/* Copyright (c) Oracle Corporation, 1998, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpmivde.xrl
*
*   DESCRIPTION
*
*       Migration script for the OEM V2 repository vde (event system) 
*       tables.  
*
*   NOTES
*
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t event_name, reg_node_name;
xp_int_t g1, g99, v1id, reg_count, node_discovered = 1;

/*******************************************************************************
**  If the V2 user doesn't have access to the event system, return without action.
*******************************************************************************/
if (!v2_evt_access)
  return;

printf(get_text(xprc_gl_wip_mig_vde_head));

/***************************************************************************
**  Perform some upfront checks.
***************************************************************************/

g1 = repos::open ("select profile_id, profile_name from evt_profile order "
                  "by profile_id");

while (repos::fetch (&g1, &v1id, event_name))
  {
    g99 = repos::open ("select node_name from evt_instance where profile_id = :1",
                        v1id);

    while (repos::fetch (&g99, reg_node_name))
      {
        makelower(reg_node_name);
        /* Check to see if the V1 event is successfully registered.   */
        node_discovered = repos::integer("select count(*) from smp_ad_nodes_ "
                                         "where LOWER(node) = :1", reg_node_name);
        if (!node_discovered)
          break;
      }

    repos::close(&g99);

    if (!repos::is_badevent(event_name) && node_discovered)
      {
        /* Check to see if the V1 event is successfully registered.   */
        reg_count = repos::integer("select count(*) from evt_instance where "
                                    "profile_id = :1 and status = 0", v1id);

        /***************************************************************************
        **  If the events in the V1 event set to be migrated are consistant for
        **  frequency, frequency units, and fix it job, they can be migrated to a
        **  single V2 event.  Otherwise each V1 event will be migrated to a 
        **  separate V2 event.
        ***************************************************************************/        
        if (((repos::integer("select count(distinct " 
              "frequency_units) from evt_profile_events "
              "where profile_id = :1", v1id) > 1)) ||
            ((repos::integer("select count(distinct "
              "frequency) from evt_profile_events "
              "where profile_id = :1", v1id) > 1)) ||
            ((repos::integer("select count(distinct "
              "fixit_job_id) from evt_profile_events "
              "where profile_id = :1", v1id) > 1)))
          {
            printf(get_text(xprc_gl_wip_mig_vde_info2), event_name);
            repos::migrate_each_event(v1id, reg_count);
          }
        else
          repos::migrate_one_event(v1id, reg_count);
      } /* end if */
  } /* end while g1 */

repos::close(&g1);


if (migrate_truncate)
  {
    printf(get_text(xprc_gl_wip_mig_vde_info4));

    repos::execute("truncate table EVT_PROFILE_EVENTS");
    repos::execute("truncate table EVT_DEST_PROFILE");
    repos::execute("truncate table EVT_HISTORY");
    repos::execute("truncate table EVT_INSTANCE");
    repos::execute("truncate table EVT_OUTSTANDING");
    repos::execute("truncate table EVT_REGISTRY");
    repos::execute("truncate table EVT_REGISTRY_BACKLOG");
    repos::execute("delete from EVT_PROFILE");
  }

return; /* Main return for smpmivde.xrl */
