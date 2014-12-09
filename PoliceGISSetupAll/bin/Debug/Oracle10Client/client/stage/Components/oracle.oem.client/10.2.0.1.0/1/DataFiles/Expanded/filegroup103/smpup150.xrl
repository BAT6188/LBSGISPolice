/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpup140.xrl
*
*   DESCRIPTION
*
*       Upgrade script for console 
*
*   NOTES
*
*       Upgrades v1.4 repository to V1.5
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t buf, buf1;
xp_int_t s1, s2, id, id2, len, parent;


/*******************************************************************************
**  Backup manager
*******************************************************************************/
repos::execute_ignore("drop table SMP_VAR_LIST_DATABASES_ ");
repos::execute("create table SMP_VAR_LIST_DATABASES_ "
               "("
               "DBNAME             	VARCHAR2(32),"
               "USERC		        VARCHAR2(32)"
               ")");

repos::execute_ignore("drop view SMP_VAR_LIST_DATABASES ");
repos::execute("create or replace view SMP_VAR_LIST_DATABASES as "
               "  select DBNAME,USERC"
               "   from SMP_VAR_LIST_DATABASES_");

repos::execute("alter table SMP_BRM_SAVED_JOB_ add (JOBTYPE VARCHAR2(32) default 'Backup Configuration')");

repos::execute_ignore("drop view SMP_BRM_SAVED_JOB");
repos::execute("create or replace view SMP_VAR_LIST_DATABASES as "
               "  select JOBNAME,JOBDESCRIPTION,DBNAME,JOBTYPE"
               "   from SMP_BRM_SAVED_JOB_");

repos::work_in_progress();

/*******************************************************************************
**  1.4 event subsystem repository into a 1.5
*******************************************************************************/

repos::execute("alter table EVT_OPERATORS add (PAGING_PREFIX	VARCHAR(80))");

repos::execute("create table EVT_CARRIER_CONFIGURATION_TEMP ( "
	 "name		VARCHAR2(40)	Primary Key, "
	 "area_code	VARCHAR2(256),"
	 "phone		VARCHAR2(256)	NOT NULL,"
	 "pager_type	NUMBER,	"
	 "protocol	NUMBER,		"
	 "connect_delay NUMBER,	"
	 "timeout_period	NUMBER,"
	 "country_id NUMBER		"	
               ")");

repos::execute("insert into EVT_CARRIER_CONFIGURATION_TEMP (name, phone) "
 	"select name, phone from EVT_CARRIER_CONFIGURATION");

repos::execute_ignore("drop table EVT_CARRIER_CONFIGURATION");
repos::execute("rename EVT_CARRIER_CONFIGURATION_TEMP to EVT_CARRIER_CONFIGURATION");

repos::execute("update EVT_CARRIER_CONFIGURATION"
	"set pager_type = 1,"
	"	protocol   = 10,"
		"connect_delay = 5,"
		"timeout_period = 60,"
		"country_id = 1");

repos::work_in_progress();
