/* Copyright (c) Oracle Corporation, 1997, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpmival.xrl
*
*   DESCRIPTION
*
*       Validation script for the target repository of an OEM migration.  
*
*   NOTES
*
*******************************************************************************/

xp_str_t user, job_name, clsid, test_name, profile_id, service_name, event_name;
xp_int_t s1, s2, version;

/***************************************************************************
**  Look for V2 repository user table.
***************************************************************************/

if (!target::table_exists ("smp_vdu_principals_table"))
  {
    return (VOBMGR_K_NOT_FOUND);
  }

/***************************************************************************
**  Make sure the target V2 repository has correct version - 2.1.0.1.0
***************************************************************************/

s1 = target::open("select version from SMP_VDS_REPOS_VERSION "
                  "where app_name = '/com/oracle/Sysman/EM/EMSystem'");

if (target::fetch(&s1, &version))
  {
    if (version < 3)
      {
        target::close(&s1);
	      return (VOBMGR_K_INVALID_VERSION);
      } 
  }

target::close(&s1);

/***************************************************************************
**  Build V2 repository user list.
***************************************************************************/

s1 = target::open ("select principal_name from SMP_VDU_PRINCIPALS_TABLE");

while (target::fetch (&s1, user))
  {
    repos::v2user(user);
  }

target::close(&s1);

/***************************************************************************
**  Build the good task key list.  Job system.
***************************************************************************/

repos::good_taskkey("OracleEMBroadcast");
repos::good_taskkey("OracleEMRunDBA");
repos::good_taskkey("OracleEMRunOSCommand");
repos::good_taskkey("OracleEMRunSQLPlus");
repos::good_taskkey("OracleEMRunTCL");
repos::good_taskkey("OracleEMShutdownDatabase");
repos::good_taskkey("OracleEMShutdownListener");
repos::good_taskkey("OracleEMStartupDatabase");
repos::good_taskkey("OracleEMStartupListener");

/***************************************************************************
**  Get the good task CLSID list. Job system.
***************************************************************************/

repos::getgoodtasks();

/***************************************************************************
**  Build a list of jobs that won't migrate to V2 because of bad tasks.
***************************************************************************/

s1 = repos::open ("select DISTINCT job.name,clsid from smp_job job," 
                   "smp_job_task_instance task where job.id = task.id");

while (repos::fetch (&s1, job_name, clsid))
  {
    if (!repos::check_taskclsid(clsid))
      {
	      repos::bad_job(job_name);
      }
  }

repos::close(&s1);

/***************************************************************************
**  Build the good test list.  Event system.
***************************************************************************/

repos::good_test("/oracle/host/fault/updown");
repos::good_test("/oracle/host/perf/cpuutil");
repos::good_test("/oracle/host/perf/paging");
repos::good_test("/oracle/host/space/diskfull");
repos::good_test("/oracle/host/space/swapfull");
repos::good_test("/oracle/rdbms/fault/alert");
repos::good_test("/oracle/rdbms/fault/userblk");
repos::good_test("/oracle/rdbms/fault/updown");
repos::good_test("/oracle/rdbms/fault/probe");
repos::good_test("/oracle/rdbms/perf/bufcache");
repos::good_test("/oracle/rdbms/perf/chainrow");
repos::good_test("/oracle/rdbms/perf/ddcache");
repos::good_test("/oracle/rdbms/perf/diskio");
repos::good_test("/oracle/rdbms/perf/libcache");
repos::good_test("/oracle/rdbms/perf/netio");
repos::good_test("/oracle/rdbms/perf/sysstata");
repos::good_test("/oracle/rdbms/perf/sysstatd");
repos::good_test("/oracle/rdbms/resource/dfilelmt");
repos::good_test("/oracle/rdbms/resource/locklmt");
repos::good_test("/oracle/rdbms/resource/proclmt");
repos::good_test("/oracle/rdbms/resource/sesslmt");
repos::good_test("/oracle/rdbms/resource/userlmt");
repos::good_test("/oracle/rdbms/space/archfull");
repos::good_test("/oracle/rdbms/space/chunksml");
repos::good_test("/oracle/rdbms/space/dumpfull");
repos::good_test("/oracle/rdbms/space/maxext");
repos::good_test("/oracle/sqlnet/fault/updown");
repos::good_test("/oracle/host/unsolicited_event/unsolicited_event");
repos::good_test("/oracle/host/fault/dgalert");
repos::good_test("/oracle/host/fault/dgupdown");
repos::good_test("/oracle/rdbms/fault/alert");
repos::good_test("/oracle/rdbms/fault/archhung");
repos::good_test("/oracle/rdbms/fault/brknjob");
repos::good_test("/oracle/rdbms/fault/blkcorrupt");
repos::good_test("/oracle/rdbms/fault/deftran");
repos::good_test("/oracle/rdbms/fault/deferror");
repos::good_test("/oracle/rdbms/fault/faildjob");
repos::good_test("/oracle/rdbms/fault/sessterm");
repos::good_test("/oracle/rdbms/fault/unschdjob");
repos::good_test("/oracle/rdbms/perf/freebuf");
repos::good_test("/oracle/rdbms/perf/memsort");
repos::good_test("/oracle/rdbms/perf/idxrebuild");
repos::good_test("/oracle/rdbms/perf/redolog");
repos::good_test("/oracle/rdbms/perf/rollback");
repos::good_test("/oracle/rdbms/space/alertlrg");
repos::good_test("/oracle/rdbms/space/fseggrth");
repos::good_test("/oracle/rdbms/space/multext");
repos::good_test("/oracle/rdbms/space/snpshtsz");
repos::good_test("/oracle/rdbms/space/tbspfull");
repos::good_test("/oracle/rdbms/audit/usradt");
repos::good_test("*");

/***************************************************************************
**  Build a list of events that won't migrate to V2 because of bad tests or
**  service types.
***************************************************************************/

s2 = repos::open ("select DISTINCT ep.profile_id, ep.profile_name, "
                  "ep.service_name, epe.event_name from evt_profile ep, "
                  "evt_profile_events epe where ep.profile_id = epe.profile_id");

while (repos::fetch (&s2, profile_id, event_name, service_name, test_name))
  {
    if (!repos::check_test(test_name))
      {
	      repos::bad_event(event_name);
      }
    else if (!repos::check_service_name(service_name))
      {
	      repos::bad_event_service(event_name);
      }
  }

repos::close(&s2);

return (VOBMGR_K_OK);