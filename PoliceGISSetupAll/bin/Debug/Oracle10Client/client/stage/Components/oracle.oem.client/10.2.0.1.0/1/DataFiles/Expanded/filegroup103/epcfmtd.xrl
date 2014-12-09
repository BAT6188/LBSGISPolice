/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       epcfmtd.xrl
*
*   DESCRIPTION
*
*       Drop formatted tables
*
*   NOTES
*
*******************************************************************************/

/*******************************************************************************
**  Purge old format tables
*******************************************************************************/

if (repos::table_exists("EPC_EVENT"))
  {
    xp_int_t s1;
    xp_char_t name;

    s1 = repos::open ("select event_table_name from epc_event");

    while (success == repos::fetch (&s1, name))
      {
        repos::execute_ignore(concat("drop table ", name));
      }

    repos::close (&s1);
  }

repos::work_in_progress();

repos::execute_ignore("drop table epc_process");
repos::execute_ignore("drop table epc_facility");
repos::execute_ignore("drop table epc_event");
repos::execute_ignore("drop table epc_item");
repos::execute_ignore("drop table epc_event_item");
repos::execute_ignore("drop table epc_facility_registration");
repos::execute_ignore("drop table epc_collection");
repos::execute_ignore("drop table xp_epc_control");

repos::work_in_progress();

repos::execute_ignore("drop sequence epc_collection_id");

repos::work_in_progress();

repos::delete_version("EPCFMT");

repos::commit();

