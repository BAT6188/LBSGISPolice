/* Copyright (c) Oracle Corporation, 1998, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpmivdj.xrl
*
*   DESCRIPTION
*
*       Migration script for the OEM V2 repository vdj (job system) 
*       tables.  
*
*   NOTES
*
*
*******************************************************************************/

/*******************************************************************************
**  If the V2 user doesn't have access to the job system, return without action.
*******************************************************************************/
if (!v2_job_access)
  return;

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t job_name, job_desc, service_type, schedule, islib, submit;
xp_str_t service_name, node_name, exec_num, timestamp, log_status, clsid;
xp_str_t min_time, max_time, task_type, task_seq, tmp_job_name, output_text;
xp_str_t previous_job_name, job_name_suffix, gmt_select, timemlstr, inst_state;
xp_str_t token[10], hours, days;
xp_int_t s1, s2, s3, s4, s5, job_id, timezone_offset, v1id, v2id, task_dep;
xp_int_t vdi_id, vdu_id, prin_id, type_count, task_count, job_name_count, isfixedit;
xp_int_t task_level, s5a, out_text_len, vxa_id, output_text_id, timeml, inst_sts;
xp_int_t isdayinterval, tmphours, tmpdays;

printf(get_text(xprc_gl_wip_mig_vdj_head));

/***************************************************************************
**  Get the local timezone offset.
***************************************************************************/
timezone_offset = repos::get_timezone();

/***************************************************************************
**  Migrate the smp_vdj_job table.
***************************************************************************/

s1 = repos::open ("select * from v2_smp_vdj_migrate_1 order by name, id");

while (repos::fetch 
        (&s1, &v1id, job_name, job_desc, service_type, schedule, 
          &isfixedit, islib))
  {
    /* Assume the schedule interval is less than one day. */
    isdayinterval = 0;

    if (!repos::is_badjob(job_name))
      {
        /*  This section handles duplicate job names. */
        target::select("select count(*) from smp_vdj_job "
                       "where job_name = :1", 1, 1, job_name, 
                        &job_name_count);

        if ((job_name_count > 0) && (job_name != previous_job_name))
          {
            tmp_job_name = concat(job_name, "(%");

            target::select("select count(distinct(job_name)) from "
                "smp_vdj_job where job_name like :1", 1, 1, tmp_job_name, 
                &job_name_count);

            job_name_count = job_name_count + 1;

            tmp_job_name = job_name;

            job_name_suffix = concat("(", job_name_count, ")");

            job_name = concat(job_name, job_name_suffix);

            printf(get_text(xprc_gl_wip_mig_vdj_info1), tmp_job_name, job_name);

            previous_job_name = tmp_job_name;

            /* Flag that objects have been renamed. */
            objects_renamed = 1; 
          }
        else if ((job_name_count > 0) && (job_name == previous_job_name))
          {
            tmp_job_name = job_name;

            job_name = concat(job_name, job_name_suffix);

            previous_job_name = tmp_job_name;
          }
        else
          {
            job_name_suffix = "";
            previous_job_name = job_name;
          }

        /*  Begin the actual migration.               */
        v2id = target::integer("select smp_vdj_job_id_seq.nextval from dual");

        /*  Add V2 specific information to the V1 schedule string. */
        /*  The core of the V1 schedule string is usable in its    */
        /*  V1 form because the V1 Agent is still in use.          */
        switch (substr(schedule, 0, 4))
          {
            case "/R=I":  /* Immediate */
              schedule = concat ( 
              "oracle.sysman.emSDK.client.omsClient.ImmediateScheduleDetails|",  
               schedule);
                break;

            case "/R=O": /* Once */
              schedule = concat ( 
              "oracle.sysman.emSDK.client.omsClient.OneTimeScheduleDetails|",  
               schedule);
            break;

            case "/R=H": /* Interval */
              {
                tokenize (token, schedule, "/");
                hours = substr(token[1], 2, 2);
                tmphours = (xp_int_t) hours;

                if (tmphours > 23)
                  {
                    /* The schedule interval is larger than 23:59. */
                    isdayinterval = 1;
                    printf(get_text(xprc_gl_wip_mig_vdj_info4), job_name);
                    /* V2 cannot handle intervals greater than 23:59 at all. */
                    schedule = 
                      "oracle.sysman.emSDK.client.omsClient.ImmediateScheduleDetails|/R=I";
                  }
                else
                  {
                    schedule = concat ( 
                      "oracle.sysman.emSDK.client.omsClient.IntervalScheduleDetails|",  
                      schedule);
                      schedule = concat ( schedule, " /ET=23:59"); /*V2 needs an ET */
                  }
              }
            break;

            case "/R=D": /* Day interval */
              {
                tokenize (token, schedule, "/");
                days = substr(token[1], 2, 3);
                tmpdays = (xp_int_t) days;

                if (tmpdays > 59)
                  {
                    /* The schedule interval is larger than 59 days. */
                    isdayinterval = 1;
                    printf(get_text(xprc_gl_wip_mig_vdj_info4), job_name);
                    /* V2 cannot handle intervals greater than 59 days at all. */
                    schedule = 
                      "oracle.sysman.emSDK.client.omsClient.ImmediateScheduleDetails|/R=I";
                  }
                else
                  {
                    schedule = concat ( 
                    "oracle.sysman.emSDK.client.omsClient.DailyScheduleDetails|",  
                      schedule);
                    schedule = concat ( schedule, " /ET=23:59"); /*V2 needs an ET */
                  }
              }
            break;

            case "/R=W": /* Day(s) of week */
              {
                schedule = concat ( 
                "oracle.sysman.emSDK.client.omsClient.DayOfWeekScheduleDetails|",  
                  schedule);
                schedule = concat ( schedule, " /ET=23:59"); /*V2 needs an ET */
              }
            break;

            case "/R=M": /* Day(s) of month */
              {
                schedule = concat ( 
                "oracle.sysman.emSDK.client.omsClient.DateOfMonthScheduleDetails|",  
                  schedule);
                schedule = concat ( schedule, " /ET=23:59"); /*V2 needs an ET */
              }
            break;

            default:

            break;
          }        

        /* The next_execution field in the following select statement is adjusted to   */
        /* the GMT timezone.  The timezone offset is provided by a Java system routine.*/
        /* Times in the V2 job system are stored as GMT and adjusted with the local    */
        /* timezone offset by the retrieving routine.                                  */
        gmt_select = concat("select to_char(max(next_execution)-(((((", timezone_offset,
                            "/1000)/60)/60)/24)), 'dd-mon-yyyy hh24 mi ss') from ",
                            "v2_smp_vdj_migrate_1a where id = :1");

        repos::select(gmt_select, 1, 1, v1id, &submit);

        target::execute("insert into smp_vdj_job values "
          "(:1, :2, :3, :4, 'Not Used', :5, :6, "
          "to_date(:7,'dd-mon-yyyy:hh24:mi:ss'), :8, :9, :10, SYSDATE, :11, NULL, 0)",
          v2id, job_name, v2user, job_desc, service_type, 
          schedule, submit, isfixedit, islib, v2user, timezone_offset);

        /***************************************************************************
        **  Migrate the smp_vdj_job_target table.
        ***************************************************************************/

        s2 = repos::open ("select distinct destination from smp_job_instance "
                          "where id = :1", v1id);

        while (repos::fetch(&s2, service_name))
          {
            if (service_type == "oracle_sysman_node")
              {
                service_name = repos::gethostname(service_name);
              }

            /* Only library jobs have a record inserted here. */
            if (islib == "1")
              {
                target::execute("insert into smp_vdj_job_target values "
                            "(:1, :2, :3)", v2id, service_name, service_type);
              }
          }

        repos::close(&s2);

        /***************************************************************************
        **  Migrate the smp_vdj_job_log table.
        ***************************************************************************/

        /* The timestamp field in the following select statement is adjusted to        */
        /* the GMT timezone.  The timezone offset is provided by a Java system routine.*/
        /* Times in the V2 job system are stored as GMT and adjusted with the local    */
        /* timezone offset by the retrieving routine.                                  */
        gmt_select = concat("select destination, execution, status, "
                            "to_char(timestamp-(((((", timezone_offset,
                            "/1000)/60)/60)/24)), 'dd-mon-yyyy hh24 mi ss'), "
                            "output_data from v2_smp_vdj_migrate_2 where job_id = :1");

        s3 = repos::open (gmt_select, v1id);

        while (repos::fetch(&s3, service_name, exec_num, log_status, 
                                                  timestamp, &output_text_id))
          {
            if (output_text_id != 0)
              {
                repos::select("select data from smp_long_text where id = :1",
                              1, 1, output_text_id, &output_text);
                                              
                /*
				vxa_id = target::integer("select "
                            "smp_vxa_blob_id_seq.nextval from dual");
				*/
				vxa_id = target::integer("select "
                            "SMP_VDJ_JOB_OUTPUT_SEQ.nextval from dual");

                repos::migrate_output_text(output_text, strlen(output_text), vxa_id);
              }
            else
              vxa_id = 0;

            if (service_type == "oracle_sysman_node")
              {
                service_name = repos::gethostname(service_name);
              }

            target::execute("insert into smp_vdj_job_log values "
                            "(:1, :2, :3, :4, to_date(:5,'dd-mon-yyyy:hh24:mi:ss'), :6, :7)", 
                            v2id, service_name, exec_num, log_status, timestamp, 
                            timezone_offset, vxa_id);
          }
        
        repos::close(&s3);     

        /***************************************************************************
        **  Migrate the smp_vdj_job_per_target table.
        ***************************************************************************/
        target::select("select to_char(max(time_stamp), 'dd-mon-yyyy:hh24:mi:ss') "
                       "from smp_vdj_job_log where job_id = :1", 
                       1, 1, v2id, &min_time);

        target::select("select to_char(min(time_stamp), 'dd-mon-yyyy:hh24:mi:ss') "
                       "from smp_vdj_job_log where job_id = :1", 
                       1, 1, v2id, &max_time);

        s4 = repos::open ("select node, destination, status, state "
                          "from v2_smp_vdj_migrate_3 "
                          "where job_id = :1", v1id);

        while (repos::fetch(&s4, node_name, service_name, &inst_sts, inst_state))
          {
            node_name = repos::gethostname(node_name);
 
            if (service_type == "oracle_sysman_node")
              {
                service_name = repos::gethostname(service_name);
              }

            /* If the job instance was active and had a scheduled status... */
            if ((inst_state == "ACTIVE") && (!isfixedit) && (!isdayinterval))
              {
                target::execute("insert into smp_vdj_job_per_target values "
                                "(:1, :2, :3, :4, :5, '', :6, "
                                "to_date(:7,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:8,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:9,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:10,'dd-mon-yyyy:hh24:mi:ss'), "
                                ":11, 1, '', '', '')", 
                                v2id, service_name, job_name, service_type, 
                                node_name, exec_num, min_time, max_time, max_time, 
                                min_time, timezone_offset);

                /***********************************************************************
                **  Load the SMP_VDD_OPERATIONS_TABLE with a job submittal request.
                ***********************************************************************/
                timemlstr = repos::get_timesec();

                timemlstr = concat(timemlstr, "000");

                target::execute("insert into smp_vdd_operations_table values (:1, "
                                "'SubmitJob', 'VdjJob', :2, :3, :4,'OP_UNALLOCATED', 'vdj', :5, 'Y')", 
                                v2id, service_name, node_name, v2user, timemlstr);
              } 
            else if ((inst_state == "ACTIVE") && (isfixedit))
              {
                target::execute("insert into smp_vdj_job_per_target values "
                                "(:1, :2, :3, :4, :5, '', :6, "
                                "to_date(:7,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:8,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:9,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:10,'dd-mon-yyyy:hh24:mi:ss'), "
                                ":11, 15, '', '', '')", 
                                v2id, service_name, job_name, service_type, 
                                node_name, exec_num, min_time, max_time, max_time, 
                                min_time, timezone_offset);

                printf(get_text(xprc_gl_wip_mig_vdj_info3), job_name);
              }
            else if ((inst_state == "ACTIVE") && (isdayinterval))
              {
                target::execute("insert into smp_vdj_job_per_target values "
                                "(:1, :2, :3, :4, :5, '', :6, "
                                "to_date(:7,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:8,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:9,'dd-mon-yyyy:hh24:mi:ss'), "
                                "to_date(:10,'dd-mon-yyyy:hh24:mi:ss'), "
                                ":11, 15, '', '', '')", 
                                v2id, service_name, job_name, service_type, 
                                node_name, exec_num, min_time, max_time, max_time, 
                                min_time, timezone_offset);

                printf(get_text(xprc_gl_wip_mig_vdj_info4), job_name);
              }
            else
              target::execute("insert into smp_vdj_job_per_target values "
                              "(:1, :2, :3, :4, :5, '', :6, "
                              "to_date(:7,'dd-mon-yyyy:hh24:mi:ss'), "
                              "to_date(:8,'dd-mon-yyyy:hh24:mi:ss'), "
                              "to_date(:9,'dd-mon-yyyy:hh24:mi:ss'), "
                              "to_date(:10,'dd-mon-yyyy:hh24:mi:ss'), "
                              ":11, 15, '', '', '')", 
                              v2id, service_name, job_name, service_type, 
                              node_name, exec_num, min_time, max_time, max_time, 
                              min_time, timezone_offset);
          }
        
        repos::close(&s4);     

        /***************************************************************************
        **  Migrate the smp_vdi_object and smp_vdi_pos tables.
        **  (Task dependency tree and task parameter info.)
        ***************************************************************************/
        vdi_id = target::integer("select smp_vdi_object_id_sequence.nextval from dual");

        target::execute("insert into smp_vdi_object_table values "
                            "(:1,'JOB', :2, :3)", vdi_id, v2user, v2id);

        task_count = repos::integer ("select count(*) from smp_job_task_instance "
                                      "where id = :1", v1id);

        repos::get_taskarrays(task_count);

        s5 = repos::open ("select task_sequence, task_level, clsid "
                          "from smp_job_task_instance "
                          "where id = :1 order by task_sequence", v1id);

        while (repos::fetch(&s5, &task_seq, &task_level, clsid))
          {
            type_count = repos::task_type_count(clsid);

            repos::define_currenttask(clsid);

            task_type = repos::get_tasktype(clsid);

            /***************************************************************************
            **  This section determines whether a task is a dependant task and which
            **  task it is dependant on if it is a dependant task.
            ***************************************************************************/            
            if (task_level > 1) /* Dependant task */
              {
                s5a = repos::open ("select task_sequence from smp_job_task_instance "
                                   "where id = :1 and task_sequence < :2 and "
                                   "task_level = (:3 - 1) order by task_sequence desc",
                                    v1id, task_seq, task_level);
                repos::fetch(&s5a, &task_dep);
                repos::close(&s5a); 
              }
            else
              task_dep = -1;  /* No dependency */

            repos::migrate_task(&v1id, &task_seq, &task_type, &type_count, &task_dep);
          }
        
        repos::close(&s5);

        repos::store_tasks(v2domain, v2id);

        repos::purge_tasklist();
        repos::release_taskarrays();

        /***************************************************************************
        **  Grant full privileges on the new job to the v2user.
        ***************************************************************************/

        vdu_id = target::integer("select "
                            "smp_vdu_objects_sequence.nextval from dual");

        target::execute("insert into smp_vdu_objects_table values "
                        "(:1,'JOB', :2, :3)", vdu_id, v2user, v2id);

        target::select("select principal_id from smp_vdu_principals_table where "
                        "principal_name = :1", 1, 1, v2user, &prin_id);

        target::execute("insert into smp_vdu_privilege_table values "
                        "(:1,'FULL', :2)", prin_id, vdu_id);

      } /* end if not bad job */
      
  } /* end while s1 */

printf(get_text(xprc_gl_wip_mig_vdj_tail));

repos::close(&s1);

if (migrate_truncate)
  {
    printf(get_text(xprc_gl_wip_mig_vdj_info2));

    repos::execute("delete from SMP_JOB_");
    repos::execute("truncate table SMP_JOB_EVENTLIST_");
    repos::execute("truncate table SMP_JOB_HISTORY_");
    repos::execute("truncate table SMP_JOB_INSTANCE_");
    repos::execute("truncate table SMP_JOB_LIBRARY_");
    repos::execute("truncate table SMP_JOB_TASK_INSTANCE_");
  }