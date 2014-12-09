/* Copyright (c) Oracle Corporation, 1994, 1986.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vobup.xrl
*
*   DESCRIPTION
*
*       Updates repository version to the current product version.
*
*   NOTES
*
*
*
*******************************************************************************/

if (!repos::table_exists ("smp_rep_version"))
  {
    repos::execute ("create table smp_rep_version "
                    "(c_component varchar2(255),"
                    " c_current_version varchar2(255),"
                    " c_unused varchar2(255))");
  }

if (!repos::execute("select count(*) from smp_rep_version "
                    "where c_component = 'CONTROL'"))
  {
    repos::execute ("insert into smp_rep_version (c_component,"
                    "c_current_version,c_unused) values ('CONTROL','1.1','')");
  }

repos::update_version ("CONTROL");

repos::work_in_progress();
