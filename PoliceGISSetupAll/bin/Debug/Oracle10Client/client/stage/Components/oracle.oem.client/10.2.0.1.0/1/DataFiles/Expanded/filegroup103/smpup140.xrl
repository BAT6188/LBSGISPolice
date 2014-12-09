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
*       Upgrades v1.3 repository to V1.4
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t buf, buf1;
xp_int_t s1, s2, id, id2, len, parent;


/*******************************************************************************
**  smp_job_isntance_ table
*******************************************************************************/

repos::execute("alter table smp_job_instance_ add execution number");
repos::execute("update smp_job_instance_ i set i.execution = (select "
               "  max(h.execution) from smp_job_history h where i.id = h.id "
               "  and i.destination = h.destination "
               "  group by h.id)");

repos::execute_ignore("drop view smp_job_instance");
repos::execute("create view smp_job_instance as "
               "select id, owner,agent_job_id,node,destination,"
               "destination_type,status,next_execution,parameters,"
               "group_name,state,execution from smp_job_instance_ "
               "where owner = user with check option");

repos::execute("create index smp_long_text_index on smp_long_text_ (id)");
repos::execute("create index smp_job_history_index on smp_job_history_ (id, destination, execution)");

repos::work_in_progress();

/*******************************************************************************
**  evt_instance table
*******************************************************************************/

repos::execute("alter table evt_instance "
               "modify (destination_name varchar2(80))");

repos::work_in_progress();

/*******************************************************************************
**  evt_history table
*******************************************************************************/

repos::execute("alter table evt_history modify (object_name varchar2(80))");

repos::work_in_progress();

/*******************************************************************************
**  evt_outstanding table
*******************************************************************************/

repos::execute("alter table evt_outstanding modify (object_name varchar2(80))");

repos::work_in_progress();

/*******************************************************************************
**  smp_service_data
*******************************************************************************/

repos::execute("create table SMP_SERVICE_DATA_ "
               "("
               "OWNER          varchar2(32),"
               "SERVICE_NAME   varchar2(120),"
               "SERVICE_TYPE   varchar2(120),"
               "NODE           varchar2(120), "
               "DATA           varchar2(1024),"
               "primary key (OWNER, SERVICE_NAME, SERVICE_TYPE, NODE)"
               ")");

repos::execute("create view SMP_SERVICE_DATA as "
               "select OWNER, SERVICE_NAME, SERVICE_TYPE, NODE, DATA "
               "from SMP_SERVICE_DATA_ "
               "where USER = OWNER "
               "with check option constraint SMP_SERVICE_DATA_CNST");

repos::work_in_progress();

/*******************************************************************************
**  Backup manager stuff
*******************************************************************************/

repos::execute_ignore("drop table SMP_BRM_CHANNEL_DEVICE_");
repos::execute("create table SMP_BRM_CHANNEL_DEVICE_ "
               "("
               "CHANNELNAME             VARCHAR2(32),"
               "FORMATSTRING            VARCHAR2(256),"
               "FORMATDEST              VARCHAR2(256),"
               "CHANNELDEVICE           VARCHAR2(8),"
               "DEVICENAME              VARCHAR2(32),"
               "DEVICETYPE              VARCHAR2(32),"
               "PARAMETERS              VARCHAR2(32),"
               "LIMITSIZE               VARCHAR2(8),"
               "READRATE                VARCHAR2(8),"
               "OPENFILES               VARCHAR2(8),"
               "DBNAME                  VARCHAR2(32)"
               ")");

repos::execute_ignore("drop view SMP_BRM_CHANNEL_DEVICE");
repos::execute("create view SMP_BRM_CHANNEL_DEVICE as "
               "  select CHANNELNAME, FORMATSTRING, FORMATDEST,"
               "   CHANNELDEVICE, DEVICENAME, DEVICETYPE, PARAMETERS,"
               "   LIMITSIZE, READRATE, OPENFILES, DBNAME"
               "   from SMP_BRM_CHANNEL_DEVICE_");

repos::execute_ignore("drop table SMP_BRM_DEFAULT_CHANNEL_");
repos::execute("create table SMP_BRM_DEFAULT_CHANNEL_ "
               "("
               "CHANNELNAME             VARCHAR2(32),"
               "DBNAME                  VARCHAR2(32)"
               ")");

repos::execute_ignore("drop view SMP_BRM_DEFAULT_CHANNEL ");
repos::execute("create view SMP_BRM_DEFAULT_CHANNEL as "
               "  select CHANNELNAME, DBNAME"
               "   from SMP_BRM_DEFAULT_CHANNEL_");

repos::execute_ignore("drop table SMP_BRM_RC_CONNECT_STRING_ ");
repos::execute("create table SMP_BRM_RC_CONNECT_STRING_ "
               "("
               "DBNAME                  VARCHAR2(32),"
               "USERNAME                VARCHAR2(255),"
               "PASSWORD                RAW(255),"
               "SERVICE                 VARCHAR2(255)"
               ")");

repos::execute_ignore("drop view SMP_BRM_RC_CONNECT_STRING ");
repos::execute("create view SMP_BRM_RC_CONNECT_STRING as "
               "  select DBNAME, USERNAME, PASSWORD, SERVICE"
               "   from SMP_BRM_RC_CONNECT_STRING_");

repos::execute_ignore("drop table SMP_BRM_TEMP_SCRIPTS_ ");
repos::execute("create table SMP_BRM_TEMP_SCRIPTS_ "
               "("
               "JOBID                   VARCHAR2(32),"
               "TCLSCRIPT               VARCHAR2(512),"
               "RMANSCRIPT              VARCHAR2(512)"
               ")");

repos::execute_ignore("drop view SMP_BRM_TEMP_SCRIPTS ");
repos::execute("create view SMP_BRM_TEMP_SCRIPTS as "
               "  select JOBID, TCLSCRIPT, RMANSCRIPT"
               "   from SMP_BRM_TEMP_SCRIPTS_");

repos::execute_ignore("drop SEQUENCE smp_brm_id ");
repos::execute("CREATE SEQUENCE smp_brm_id MAXVALUE 9999 CYCLE ORDER");

repos::execute_ignore("drop table SMP_BRM_ACTIVE_JOB_ ");
repos::execute("create table SMP_BRM_ACTIVE_JOB_ "
               "("
               "JOBID               VARCHAR2(32),"
               "TARGETDB                VARCHAR2(32),"
               "JOBNAME             VARCHAR2(32)"
               ")");

repos::execute_ignore("drop view SMP_BRM_ACTIVE_JOB ");
repos::execute("create view SMP_BRM_ACTIVE_JOB as "
               "  select JOBID,TARGETDB, JOBNAME"
               "   from SMP_BRM_ACTIVE_JOB_");

repos::execute_ignore("drop table SMP_BRM_SAVED_JOB_ ");
repos::execute("create table SMP_BRM_SAVED_JOB_ "
               "("
               "JOBNAME                 VARCHAR2(32),"
               "JOBDESCRIPTION            VARCHAR2(32),"
               "DBNAME                  VARCHAR2(32)"
               ")");

repos::execute_ignore("drop view SMP_BRM_SAVED_JOB ");
repos::execute("create view SMP_BRM_SAVED_JOB as "
               "  select JOBNAME,JOBDESCRIPTION,DBNAME"
               "   from SMP_BRM_SAVED_JOB_");

repos::execute_ignore("drop table SMP_VAB_ACTIVE_JOB_ ");
repos::execute("create table SMP_VAB_ACTIVE_JOB_ "
               "("
               "JOBID               VARCHAR2(32),"
               "TARGETDB            VARCHAR2(32),"
               "JOBNAME             VARCHAR2(32)"
               ")");


repos::execute_ignore("drop view SMP_VAB_ACTIVE_JOB ");
repos::execute("create view SMP_VAB_ACTIVE_JOB as "
               "  select JOBID,TARGETDB, JOBNAME"
               "   from SMP_VAB_ACTIVE_JOB_");

repos::execute_ignore("drop table SMP_VAB_SAVED_JOB_ ");
repos::execute("create table SMP_VAB_SAVED_JOB_ "
               "("
               "JOBNAME                 VARCHAR2(32),"
               "JOBDESCRIPTION          VARCHAR2(32),"
               "DBNAME                  VARCHAR2(32)"
               ")");

repos::execute_ignore("drop view SMP_VAB_SAVED_JOB ");
repos::execute("create view SMP_VAB_SAVED_JOB as "
               "  select JOBNAME,JOBDESCRIPTION,DBNAME"
               "   from SMP_VAB_SAVED_JOB_");

repos::execute_ignore("drop table SMP_EBU_ACTIVE_JOB_ ");
repos::execute("create table SMP_EBU_ACTIVE_JOB_ "
               "("
               "JOBID             	VARCHAR2(32),"
               "TARGETDB            	VARCHAR2(32),"
               "JOBNAME           	VARCHAR2(32)"
               ")");

repos::execute_ignore("drop view SMP_EBU_ACTIVE_JOB ");
repos::execute("create or replace view SMP_EBU_ACTIVE_JOB as "
               "  select JOBID,TARGETDB, JOBNAME"
               "   from SMP_EBU_ACTIVE_JOB_");

repos::execute_ignore("drop table SMP_EBU_SAVED_JOB_ ");
repos::execute("create table SMP_EBU_SAVED_JOB_ "
               "("
               "JOBNAME             	VARCHAR2(32),"
               "JOBDESCRIPTION          VARCHAR2(32),"
               "DBNAME		        VARCHAR2(32)"
               ")");

repos::execute_ignore("drop view SMP_EBU_SAVED_JOB ");
repos::execute("create or replace view SMP_EBU_SAVED_JOB as "
               "  select JOBNAME,JOBDESCRIPTION,DBNAME"
               "   from SMP_EBU_SAVED_JOB_");

repos::work_in_progress();

/*******************************************************************************
**  Update repository version
*******************************************************************************/

repos::update_version ("CONSOLE");
