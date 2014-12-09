/* Copyright (c) Oracle Corporation, 1997, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpmivdn.xrl
*
*   DESCRIPTION
*
*       Migration script for the OEM V2 repository vdn (service discovery) tables.  
*
*   NOTES
*
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t name, node, agent, tnsaddr, type, userdata, fqnode, oms_name;
xp_int_t s1, typeid, nodeid, rowcount, timezone_offset;

printf(get_text(xprc_gl_wip_mig_vdn_head));

/***************************************************************************
 **  Get the local timezone offset.
 ***************************************************************************/
timezone_offset = repos::get_timezone();

/***************************************************************************
 **  Get the name of the OMS so we can populate the ping (VDP) service 
 **  tables correctly.  The OMS must have been started at least once
 **  prior to migration for this to work properly.  This table is populated
 **  by the OMS the first time it touches the repository.
 ***************************************************************************/
s1 = target::open ("select mas_name from smp_vdf_maslist where ROWNUM = 1");
target::fetch (&s1, oms_name);
target::close(&s1);

/***************************************************************************
 **  Migrate the smp_vdn_target_type_defn table.
 ***************************************************************************/

if (!target::integer ("select count(*) from smp_vdn_target_type_defn "
                         "where name = 'oracle_sysman_node'"))
      {
        target::execute("insert into smp_vdn_target_type_defn values "
                       "(smp_vdn_target_type_defn_seq.nextval,'oracle_sysman_node',0)");
      }

if (!target::integer ("select count(*) from smp_vdn_target_type_defn "
                         "where name = 'oracle_sysman_database'"))
      {
        target::execute("insert into smp_vdn_target_type_defn values "
                       "(smp_vdn_target_type_defn_seq.nextval,'oracle_sysman_database',0)");
      }

if (!target::integer ("select count(*) from smp_vdn_target_type_defn "
                         "where name = 'oracle_sysman_listener'"))
      {
        target::execute("insert into smp_vdn_target_type_defn values "
                       "(smp_vdn_target_type_defn_seq.nextval,'oracle_sysman_listener',0)");
      }
if (!target::integer ("select count(*) from smp_vdn_target_type_defn "
                         "where name = 'oracle_sysman_ops'"))
      {
        target::execute("insert into smp_vdn_target_type_defn values "
                       "(smp_vdn_target_type_defn_seq.nextval,'oracle_sysman_ops',0)");
      }
if (!target::integer ("select count(*) from smp_vdn_target_type_defn "
                         "where name = 'oracle_sysman_opsinst'"))
      {
        target::execute("insert into smp_vdn_target_type_defn values "
                       "(smp_vdn_target_type_defn_seq.nextval,'oracle_sysman_opsinst',0)");
      }


/***************************************************************************
 **  Migrate the smp_vdn_node_list table.
 ***************************************************************************/

if (!target::integer ("select count(*) from smp_vdp_oms_num_nodes"))
  {
    target::execute("insert into smp_vdp_oms_num_nodes values "
                   "(:1, 0)", oms_name);
  }

rowcount = target::integer ("select count(*) from smp_vdg_node_list");

s1 = repos::open ("select lower(node), userdata from v2_smp_vdn_migrate_2");

while (repos::fetch (&s1, &node, &agent))
  {
    fqnode = repos::gethostname(node);

    /* Add the node to the Agent Gateway's list.            */
    if (!target::integer ("select count(*) from smp_vdg_node_list "
                         "where nodename = :1", fqnode))
      {
        target::execute("insert into smp_vdg_node_list values "
                       "(:1, SYSDATE, 'none', 'UP', NULL, :2, LOWER(:3), :4, 'GOOD')",
                        fqnode, rowcount, node, timezone_offset);

        rowcount = rowcount + 1;

        target::execute("insert into smp_vdg_node_lock_table values "
                       "(:1, SYSDATE, NULL)",
                            fqnode);
      }

    if (!target::integer ("select count(*) from smp_vdp_nodes "
                         "where node = :1", fqnode))
      {
        target::execute("insert into smp_vdp_nodes values "
                       "(:1, SYSDATE)", fqnode);
      }

    if (!target::integer ("select count(*) from smp_vdn_node_list "
                         "where name = :1", fqnode))
      {
        target::execute("insert into smp_vdn_node_list values "
                       "(smp_vdn_target_list_seq.nextval, :1, '', :2, '')",
                            fqnode, agent);
      }
    else
      printf(get_text(xprc_gl_wip_mig_vdn_info1), fqnode);
  }

repos::close(&s1);

/***************************************************************************
 **  Migrate the smp_vdn_target_list and smp_vdn_state tables.
 ***************************************************************************/

s1 = repos::open ("select * from v2_smp_vdn_migrate_5");

while (repos::fetch (&s1, &name, &node, &tnsaddr, &type, &userdata))
  {
    makelower(node);
    fqnode = repos::gethostname(node);

    target::select("select id from smp_vdn_node_list where "
                    "name = :1", 1, 1, fqnode, &nodeid);

    target::select("select id from smp_vdn_target_type_defn where "
                    "name = :1", 1, 1, type, &typeid);

    if (type == "oracle_sysman_node")
      {
        if (!target::integer ("select count(*) from smp_vdn_target_list "
                           "where name = :1 and typeid = :2", fqnode, typeid))
          {
            // add the last argument integer '0' for totalblackout column
            target::execute("insert into smp_vdn_target_list values "
                     "(smp_vdn_target_list_seq.nextval, :1, :2, '', :3, :4, '', 1, 0)",
                         typeid, fqnode, nodeid, tnsaddr);
          }
        else
          printf(get_text(xprc_gl_wip_mig_vdn_info2), fqnode);

        if (!target::integer ("select count(*) from smp_vdn_state "
                           "where target_name = :1 and target_type = :2", fqnode, type))
          {
            target::execute("insert into smp_vdn_state values "
                     "(:1, :2, :3, 0, 'UNMONITORED')",
                         fqnode, type, fqnode);
          }
      }
    else
      {
        if (!target::integer ("select count(*) from smp_vdn_target_list "
                           "where name = :1 and typeid = :2", name, typeid))
          {
            // add the last argument integer '0' for totalblackout column
            target::execute("insert into smp_vdn_target_list values "
                   "(smp_vdn_target_list_seq.nextval, :1, :2, '', :3, :4, :5, 1, 0)",
                        typeid, name, nodeid, tnsaddr, userdata);
          }
        else
          printf(get_text(xprc_gl_wip_mig_vdn_info2), name);


        if (!target::integer ("select count(*) from smp_vdn_state "
                           "where target_name = :1 and target_type = :2", name, type))
          {
            target::execute("insert into smp_vdn_state values "
                     "(:1, :2, :3, 0, 'UNMONITORED')",
                         name, type, fqnode);
          }
      }          
  }

repos::close(&s1);

printf(get_text(xprc_gl_wip_mig_vdn_tail));
