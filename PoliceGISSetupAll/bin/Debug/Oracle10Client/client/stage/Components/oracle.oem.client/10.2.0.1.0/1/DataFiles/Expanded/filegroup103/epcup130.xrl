/* Copyright (c) Oracle Corporation, 1996, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       epcup130.xrl
*
*   DESCRIPTION
*
*       Upgrade script for Oracle Trace.  The main goal is to get rid of
*       old attributes.
*
*   NOTES
*
*******************************************************************************/

/*******************************************************************************
**  Purge old format tables
*******************************************************************************/

xp_int_t s1;
xp_char_t name;

s1 = repos::open ("select event_table_name from event");

while (success == repos::fetch (&s1, name))
  {
    if (repos::table_exists(name))
      {
        repos::execute(concat("drop table ", name));
      }
  }

repos::close (&s1);

repos::execute("drop table process");
repos::execute("drop table facility");
repos::execute("drop table event");
repos::execute("drop table item");
repos::execute("drop table event_item");
repos::execute("drop table facility_registration");
repos::execute("drop table collection");

repos::execute("drop sequence collection_id");

repos::commit();
