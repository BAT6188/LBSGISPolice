 /* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       epcfmtdo.xrl
*
*   DESCRIPTION
*
*       Drop formatted tables (old version)
*
*   NOTES
*
*******************************************************************************/

/*******************************************************************************
**  Purge old format tables
*******************************************************************************/

if (repos::table_exists("EVENT"))
  {
    xp_int_t s1;
    xp_char_t name;

    s1 = repos::open ("select event_table_name from event");

    while (success == repos::fetch (&s1, name))
      {
        repos::execute_ignore(concat("drop table ", name));
      }

    repos::close (&s1);
  }

repos::work_in_progress();

repos::execute_ignore("drop table process");
repos::execute_ignore("drop table facility");
repos::execute_ignore("drop table event");
repos::execute_ignore("drop table item");
repos::execute_ignore("drop table event_item");
repos::execute_ignore("drop table facility_registration");
repos::execute_ignore("drop table collection");
repos::execute_ignore("drop table xp_epc_control");

repos::work_in_progress();

repos::execute_ignore("drop sequence collection_id");

repos::work_in_progress();

repos::delete_version("EPCFMT");

repos::commit();

