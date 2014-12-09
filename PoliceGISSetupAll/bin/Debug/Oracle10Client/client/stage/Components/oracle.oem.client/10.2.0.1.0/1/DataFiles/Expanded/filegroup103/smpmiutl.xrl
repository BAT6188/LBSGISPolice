/* Copyright (c) Oracle Corporation, 1998, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpmiutl.xrl
*
*   DESCRIPTION
*
*       Common migration script for the OEM V2 repository.  
*
*   NOTES
*
*
*******************************************************************************/

/*******************************************************************************
*
*   Routine:
*
*       migrate_each_event
*
*   Description:
*
*       Migrates each event in a V1 event set to a separate V2 event.
*
*   Formal parameters:
*
*       v1id - The V1 event set id.
*
*   Returns:
*
*       none
*
*******************************************************************************/

replace procedure repos::migrate_each_event (v1id, reg_count)

  {
    xp_str_t event_name, event_desc, target_type, sched_prefix, freq;
    xp_str_t freq_units, company, org, product, filename, tmp_evt_name;
    xp_str_t fixit_id, tmp_event_name, schedule, fixit_name, vdd_target;
    xp_str_t test_name, params, node_name, target_name, timemlstr;
    xp_int_t p1, p2, vdi_id1, vdu_id1, v2id1, event_name_count, test_count;
    xp_int_t type_count, param_count, test_seq, prin_id, v2id2, tmp_freq;
    xp_int_t vdi_id2, vdu_id2, type_id, timezone_offset, timeml;//, day_interval;

    /***************************************************************************
    **  Get the local timezone offset.
    ***************************************************************************/
    timezone_offset = repos::get_timezone();

    /***************************************************************************
    **  Migrate the smp_vde_event and smp_vde_event_details tables in parallel
    **  for each event.  This will flatten the V1 event set.
    ***************************************************************************/
    p1 = repos::open ("select * from v2_smp_vde_migrate_1 where profile_id = "
                      ":1 order by event_id", v1id);

    while (repos::fetch(&p1, &v1id, event_name, event_desc,target_type, 
                         sched_prefix, freq, freq_units, &test_seq, company, 
                         org, product, filename, &param_count, params, fixit_id))
      {
        // day_interval = 0;

        v2id1 = target::integer("select smp_vde_event_seq.nextval from dual");

        if (reg_count > 0)
          v2id2 = target::integer("select smp_vde_event_seq.nextval from dual");
        
        test_seq = 0;  /* One test per event. */

        tmp_evt_name = event_name;

        /*  Build the new event name. */
        event_name = concat (event_name, "_", filename);

        /*  This section handles duplicate event names. */
        target::select("select count(*) from smp_vde_event "
                       "where name = :1", 1, 1, event_name, 
                        &event_name_count);

        event_desc = repos::removeCR(event_desc);

        if (event_name_count > 0)
          {
            tmp_event_name = concat(event_name, "(%");

            target::select("select count(*) from smp_vde_event "
               "where name like :1", 1, 1, tmp_event_name, 
                &event_name_count);

            event_name_count = event_name_count + 1;

            event_name = concat (event_name, "(", event_name_count, ")");
          }

        printf(get_text(xprc_gl_wip_mig_vde_info3), tmp_evt_name, filename, event_name);

        /* This section determines the appropriate agent schedule string. */
        target_type = repos::get_nls_service(target_type);

        freq_units = repos::get_nls_interval(freq_units);

        trimleft(freq);
        trimright(freq);

        switch (freq_units)
          {
            case "D":   /* Days */
              {
                freq = repos::get_time_interval(freq_units, freq);
                freq_units = "H";
                // day_interval = 1;
              }
            break;

            case "H":
              {
                freq = repos::get_time_interval(freq_units, freq);
                // tmp_freq = (xp_int_t) freq;
                // if (tmp_freq > 23)
                //  day_interval = 1;
              }
            break;

            case "M":
              {
                freq = repos::get_time_interval(freq_units, freq);
                freq_units = "H";
              }
            break;

            case "S":
              {
                tmp_freq = (xp_int_t) freq;
                if (tmp_freq < 60)
                  freq = "60";
                freq = repos::get_time_interval(freq_units, freq);
                freq_units = "H"; 
              }
            break;

            default:
              {
                freq_units = "H";
                freq = repos::get_time_interval(freq_units, freq);
              }
            break;
          }

        schedule = concat( sched_prefix, freq_units, " /F=", freq, 
                                      " /ED=04/25/3980 /ET=12:00");

        /* Events with intervals larger than 23:59 are not supported in V2. */
        /* A restriction in this area was removed in 2.1. */
        // if (day_interval)
        //  printf(get_text(xprc_gl_wip_mig_vde_info7), event_name);

        /*  This section gets the V2 fix it job. */
        if (fixit_id > 0)
          {
            repos::select("select name from smp_job "
                          "where id = :1", 1, 1, fixit_id, 
                          fixit_name);

            target::select("select max(job_id) from smp_vdj_job "
                           "where job_name = :1 and is_lib = 0", 1, 1, 
                           fixit_name, &fixit_id);

            if ( reg_count > 0)
              printf(get_text(xprc_gl_wip_mig_vde_info6), event_name, fixit_name);
            else
              printf(get_text(xprc_gl_wip_mig_vde_info5), event_name, fixit_name);
          }
        else
          {
            fixit_id = 0;
            fixit_name = "";
          }

        /* Perform the insert into smp_vde_event */
        target::execute("insert into smp_vde_event values "
          "(:1, :2, :3, :4, :5, :6, '', 'Y', 0, '', '', 'N', 0, 'N')",
          v2id1, event_name, v2user, event_desc, target_type, 
          schedule);

        /* Perform the insert into smp_vde_event for the registered event */
        if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))          
          target::execute("insert into smp_vde_event values "
            "(:1, :2, :3, :4, :5, :6, '', 'N', 0, '', '', 'N', 0, 'N')",
            v2id2, event_name, v2user, event_desc, target_type, 
            schedule);
                    
        /***************************************************************************
        **  Migrate the smp_vdi_object and smp_vdi_pos tables.
        **  (Test dependency tree and test parameter info.)
        ***************************************************************************/
        vdi_id1 = target::integer("select smp_vdi_object_id_sequence.nextval "
                                  "from dual");

        if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
          vdi_id2 = target::integer("select smp_vdi_object_id_sequence.nextval "
                                    "from dual");

        target::execute("insert into smp_vdi_object_table values "
                            "(:1,'EVENT', :2, :3)", vdi_id1, v2user, v2id1);

        if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
          target::execute("insert into smp_vdi_object_table values "
                          "(:1,'EVENT', :2, :3)", vdi_id2, v2user, v2id2);          

        test_count = 1; /* Each event only has one test. */

        repos::get_testarrays(test_count);

        /* Accept 3rd party event changes to unsolicited in V2 */
        if (filename == "*")
          {
            company   = "oracle";
            org       = "host";
            product   = "unsolicited_event";
            filename  = "unsolicited_event";

            target::execute("update smp_vde_event set is_unsolicited = 'Y' "
                            "where id = :1 and name = :2", v2id1, event_name);
          }

        /* Perform the insert into smp_vde_event_details */
        target::execute("insert into smp_vde_event_details values "
          "(:1, :2, :3, :4, :5, :6)",
          v2id1, test_seq, company, org, product, filename);

        /* Perform the insert into smp_vde_event_details */
        if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
          target::execute("insert into smp_vde_event_details values "
            "(:1, :2, :3, :4, :5, :6)",
            v2id2, test_seq, company, org, product, filename);
 
        test_name = concat(company, "/", org, "/", product, "/", filename);

        type_count = repos::test_type_count(test_name);

        repos::define_currenttest(test_name);

        /* Set up for the inserts into smp_vdi_pos. */
        repos::migrate_test(test_seq, company, org, product, filename, 
                                    type_count, param_count, params);

        /* Perform the inserts into smp_vdi_pos. */
        repos::store_tests(v2domain, v2id1);

        if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
          repos::store_tests(v2domain, v2id2);              

        repos::purge_testlist();
        repos::release_testarrays();

        /***************************************************************************
        **  Grant full privileges on the new event to the v2user.
        ***************************************************************************/
        vdu_id1 = target::integer("select "
                            "smp_vdu_objects_sequence.nextval from dual");

        target::execute("insert into smp_vdu_objects_table values "
                        "(:1,'EVENT', :2, :3)", vdu_id1, v2user, v2id1);

        target::select("select principal_id from smp_vdu_principals_table where "
                        "principal_name = :1", 1, 1, v2user, &prin_id);

        target::execute("insert into smp_vdu_privilege_table values "
                        "(:1,'FULL', :2)", prin_id, vdu_id1);

        if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval)) /* Do it again for the registered event instance */
          {
            vdu_id2 = target::integer("select "
                                "smp_vdu_objects_sequence.nextval from dual");

            target::execute("insert into smp_vdu_objects_table values "
                            "(:1,'EVENT', :2, :3)", vdu_id2, v2user, v2id2);

            target::execute("insert into smp_vdu_privilege_table values "
                            "(:1,'FULL', :2)", prin_id, vdu_id2);
          }
                      
        /***************************************************************************
        **  Load the SMP_VDE_EVENT_TARGET_INFO, SMP_VDE_EVENT_TARGET_STATUS and 
        **  SMP_VDE_EVENT_TARGET_DETAILS tables only for registered events.
        ***************************************************************************/
        if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
          {        
            p2 = repos::open ("select distinct node_name, destination_name from "
                              "evt_instance where profile_id = "
                              ":1 and status = 0", v1id);

            while (repos::fetch(&p2, node_name, target_name))
              {
                node_name = repos::gethostname(node_name);

                target::select("select typeid from smp_vdn_target_list "
                               "where name = :1", 1, 1, target_name, &type_id);

                target::select("select name from smp_vdn_target_type_defn "
                               "where id = :1", 1, 1, &type_id, target_type);
                                
                if (target_type == "oracle_sysman_node")
                  {
                    target_name = repos::gethostname(target_name);
                  }

                target::execute("insert into smp_vde_event_target_info values (:1, :2, :3, "
                                "null, null, null, null, null, null)", v2id1, target_name, 
                                target_type);

                target::execute("insert into smp_vde_event_target_info values (:1, :2, :3, "
                                ":4, 204, null, null, null, null)", v2id2, target_name, 
                                target_type, node_name);

                target::execute("insert into smp_vde_event_target_state values (:1, :2, :3, "
                                ":4, 15, 302)", v2id2, target_name, 
                                target_type, node_name);

                target::execute("insert into smp_vde_event_target_details values "
                            "(:1, :2, :3, :4, :5, :6, :7, :8, null, 15, null, null, null)",
                             v2id2, target_name, target_type, node_name, company, org, 
                             product, filename);

                /***********************************************************************
                **  Load the SMP_VDD_OPERATIONS_TABLE with a registration request.
                ***********************************************************************/
                timemlstr = repos::get_timesec();

                /* Convert to milliseconds      */
                timemlstr = concat(timemlstr, "000");

                vdd_target = concat(target_name, "|", target_type);

                target::execute("insert into smp_vdd_operations_table values (:1, 1, "
                                "'EVENT', :2, :3, :4,'OP_UNALLOCATED', 'vde', :5, 'Y')", v2id2, 
                                vdd_target, node_name, v2user, timemlstr);
                                                                
              } /* end while fetch p2 */

            repos::close(&p2);

          } /* end if reg_count */

      } /* While fetch p1*/

    repos::close(&p1);

    /* Flag that objects have been renamed. */
    objects_renamed = 1; 

  } /* End migrate_each_event */


/*******************************************************************************
*
*   Routine:
*
*       migrate_one_event
*
*   Description:
*
*       Migrates a V1 event set to a single V2 event.
*
*   Formal parameters:
*
*       v1id - The V1 event set id.
*
*   Returns:
*
*       none
*
*******************************************************************************/

replace procedure repos::migrate_one_event (v1id, reg_count)

  {
    xp_str_t event_name, event_desc, target_type, sched_prefix, freq;
    xp_str_t freq_units, company, org, product, filename, tmp_evt_name;
    xp_str_t fixit_id, tmp_event_name, schedule, fixit_name, vdd_target;
    xp_str_t test_name, params, node_name, target_name, timemlstr;
    xp_int_t p1, p2, vdi_id1, vdu_id1, v2id1, event_name_count, test_count;
    xp_int_t type_count, param_count, test_seq, prin_id, v2id2;//, day_interval = 0;
    xp_int_t vdi_id2, vdu_id2, type_id, timezone_offset, timeml, tmp_freq;

    /***************************************************************************
    **  Get the local timezone offset.
    ***************************************************************************/
    timezone_offset = repos::get_timezone();

    v2id1 = target::integer("select smp_vde_event_seq.nextval from dual");

    if (reg_count > 0)
      v2id2 = target::integer("select smp_vde_event_seq.nextval from dual");

    /***************************************************************************
    **  Migrate the smp_vde_event table.
    ***************************************************************************/
    repos::select ("select distinct profile_name, profile_description,"
                   "service_name, schedule_prefix, frequency, frequency_units,"
                   "fixit_job_id from v2_smp_vde_migrate_1 where profile_id = "
                   ":1", 1, 8, v1id, event_name, event_desc, target_type, 
                   sched_prefix, freq, freq_units, fixit_id);

    /*  This section handles duplicate event names. */
    target::select("select count(*) from smp_vde_event "
                   "where name = :1", 1, 1, event_name, 
                    &event_name_count);

    event_desc = repos::removeCR(event_desc);

    tmp_evt_name = event_name;

    if (event_name_count > 0)
      {
        tmp_event_name = concat(event_name, "(%");

        target::select("select count(*) from smp_vde_event "
           "where name like :1", 1, 1, tmp_event_name, 
            &event_name_count);

        event_name_count = event_name_count + 1;

        event_name = concat (event_name, "(", event_name_count, ")");

        printf(get_text(xprc_gl_wip_mig_vde_info1), tmp_evt_name, event_name);

        /* Flag that objects have been renamed. */
        objects_renamed = 1; 
      }

    /* This section determines the appropriate agent schedule string. */
    target_type = repos::get_nls_service(target_type);

    freq_units = repos::get_nls_interval(freq_units);

    trimleft(freq);
    trimright(freq);

    switch (freq_units)
      {
        case "D":   /* Days */
          {
            freq = repos::get_time_interval(freq_units, freq);
            freq_units = "H";
            // day_interval = 1;
          }
        break;

        case "H":
          {
            freq = repos::get_time_interval(freq_units, freq);
            // tmp_freq = (xp_int_t) freq;
            // if (tmp_freq > 23)
            //  day_interval = 1;
          }
        break;

        case "M":
          {
            freq = repos::get_time_interval(freq_units, freq);
            freq_units = "H";
          }
        break;

        case "S":
          {
            tmp_freq = (xp_int_t) freq;
            if (tmp_freq < 60)
              freq = "60";
              freq = repos::get_time_interval(freq_units, freq);
              freq_units = "H"; 
          } 
        break;

        default:
          {
            freq_units = "H";
            freq = repos::get_time_interval(freq_units, freq);
          }
        break;
      } /* End schedule switch */

    schedule = concat( sched_prefix, freq_units, " /F=", freq, 
                                  " /ED=04/25/3980 /ET=12:00");

    /* Events with intervals larger than 23:59 are not supported in V2. */
    /* A restriction in this area was removed in 2.1. */
    // if (day_interval)
    //  printf(get_text(xprc_gl_wip_mig_vde_info7), event_name);

    /*  This section gets the V2 fix it job. */
    if (fixit_id > 0)
      {
        repos::select("select name from smp_job "
                      "where id = :1", 1, 1, fixit_id, 
                      fixit_name);
        target::select("select max(job_id) from smp_vdj_job "
                       "where job_name = :1 and is_lib = 0", 1, 1, 
                       fixit_name, &fixit_id);

        if ( reg_count > 0)
          printf(get_text(xprc_gl_wip_mig_vde_info6), event_name, fixit_name);
        else
          printf(get_text(xprc_gl_wip_mig_vde_info5), event_name, fixit_name);
      }
    else
      {
        fixit_id = 0;
        fixit_name = "";
      }

    /* Perform the insert into smp_vde_event */
    target::execute("insert into smp_vde_event values "
      "(:1, :2, :3, :4, :5, :6, '', 'Y', 0, '', '', 'N', 0, 'N')",
      v2id1, event_name, v2user, event_desc, target_type, 
      schedule);
                
    /* Perform the insert into smp_vde_event for the registered event */
    if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
      target::execute("insert into smp_vde_event values "
        "(:1, :2, :3, :4, :5, :6, '', 'N', 0, '', '', 'N', 0, 'N')",
        v2id2, event_name, v2user, event_desc, target_type, 
        schedule);

    /***************************************************************************
    **  Migrate the smp_vde_event_details table.
    ***************************************************************************/
    p1 = repos::open ("select profile_id, event_id, company, organization, "
                      "product, filename ,num_args, args from "
                      "v2_smp_vde_migrate_1 where profile_id = "
                      ":1 order by event_id", v1id);

    /***************************************************************************
    **  Migrate the smp_vdi_object and smp_vdi_pos tables.
    **  (Test dependency tree and test parameter info.)
    ***************************************************************************/
    vdi_id1 = target::integer("select smp_vdi_object_id_sequence.nextval from dual");

    if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
      vdi_id2 = target::integer("select smp_vdi_object_id_sequence.nextval "
                                "from dual");

    target::execute("insert into smp_vdi_object_table values "
                        "(:1,'EVENT', :2, :3)", vdi_id1, v2user, v2id1);

    if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
      target::execute("insert into smp_vdi_object_table values "
                      "(:1,'EVENT', :2, :3)", vdi_id2, v2user, v2id2); 

    test_count = repos::integer ("select count(*) from v2_smp_vde_migrate_1 "
                                  "where profile_id = :1", v1id);

    repos::get_testarrays(test_count);

    while (repos::fetch(&p1, &v1id, &test_seq, company, org, product, filename, 
                                                &param_count, params))
      {
        /* Accept 3rd party event changes to unsolicited in V2 */
        if (filename == "*")
          {
            company   = "oracle";
            org       = "host";
            product   = "unsolicited_event";
            filename  = "unsolicited_event";

            target::execute("update smp_vde_event set is_unsolicited = 'Y' "
                            "where id = :1 and name = :2", v2id1, event_name);
          }

        /* Perform the insert into smp_vde_event_details */
        target::execute("insert into smp_vde_event_details values "
          "(:1, :2, :3, :4, :5, :6)",
          v2id1, test_seq, company, org, product, filename);

        /* Perform the insert into smp_vde_event_details */
        if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
          target::execute("insert into smp_vde_event_details values "
            "(:1, :2, :3, :4, :5, :6)",
            v2id2, test_seq, company, org, product, filename);
      
        test_name = concat(company, "/", org, "/", product, "/", filename);

        type_count = repos::test_type_count(test_name);

        repos::define_currenttest(test_name);

        /* Set up for and perform the inserts into smp_vdi_pos. */
        repos::migrate_test(test_seq, company, org, product, filename, 
                                          type_count, param_count, params);
      }

    repos::close(&p1);

    repos::store_tests(v2domain, v2id1);

    if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval))
      repos::store_tests(v2domain, v2id2);    

    repos::purge_testlist();
    repos::release_testarrays();

    /***************************************************************************
    **  Grant full privileges on the new event to the v2user.
    ***************************************************************************/
    vdu_id1 = target::integer("select "
                        "smp_vdu_objects_sequence.nextval from dual");

    target::execute("insert into smp_vdu_objects_table values "
                    "(:1,'EVENT', :2, :3)", vdu_id1, v2user, v2id1);

    target::select("select principal_id from smp_vdu_principals_table where "
                    "principal_name = :1", 1, 1, v2user, &prin_id);

    target::execute("insert into smp_vdu_privilege_table values "
                    "(:1,'FULL', :2)", prin_id, vdu_id1);
               
    if ((reg_count > 0) && (fixit_id == "0"))// && (!day_interval)) /* Do it again for the registered event instance */
      {
        vdu_id2 = target::integer("select "
                            "smp_vdu_objects_sequence.nextval from dual");

        target::execute("insert into smp_vdu_objects_table values "
                        "(:1,'EVENT', :2, :3)", vdu_id2, v2user, v2id2);

        target::execute("insert into smp_vdu_privilege_table values "
                        "(:1,'FULL', :2)", prin_id, vdu_id2);

        /***************************************************************************
        **  Load the SMP_VDE_EVENT_TARGET_INFO, SMP_VDE_EVENT_TARGET_STATE and 
        **  SMP_VDE_EVENT_TARGET_DETAILS tables only for registered events.
        ***************************************************************************/
        p2 = repos::open ("select distinct node_name, destination_name from "
                          "evt_instance where profile_id = "
                          ":1 and status = 0", v1id);

        while (repos::fetch(&p2, node_name, target_name))
          {
            node_name = repos::gethostname(node_name);

            target::select("select typeid from smp_vdn_target_list "
                           "where name = :1", 1, 1, target_name, &type_id);

            target::select("select name from smp_vdn_target_type_defn "
                           "where id = :1", 1, 1, &type_id, target_type);
                          
            if (target_type == "oracle_sysman_node")
              {
                target_name = repos::gethostname(target_name);
              }

            target::execute("insert into smp_vde_event_target_info values (:1, :2, :3, "
                            "null, null, null, null, null, null)", v2id1, target_name, 
                            target_type);

            target::execute("insert into smp_vde_event_target_info values (:1, :2, :3, "
                            ":4, 204, null, null, null, null)", v2id2, target_name, 
                            target_type, node_name);

            target::execute("insert into smp_vde_event_target_state values (:1, :2, :3, "
                            ":4, 15, 302)", v2id2, target_name, 
                            target_type, node_name);

            target::execute("insert into smp_vde_event_target_details values "
                        "(:1, :2, :3, :4, :5, :6, :7, :8, null, 15, null, null, null)",
                         v2id2, target_name, target_type, node_name, company, org, 
                         product, filename);

            /***********************************************************************
            **  Load the SMP_VDD_OPERATIONS_TABLE with a registration request.
            ***********************************************************************/
            timemlstr = repos::get_timesec();

            /* Convert to milliseconds      */
            timemlstr = concat(timemlstr, "000");

            vdd_target = concat(target_name, "|", target_type);

            target::execute("insert into smp_vdd_operations_table values (:1, 1, "
                            "'EVENT', :2, :3, :4, 'OP_UNALLOCATED', 'vde', :5, 'Y')", v2id2, 
                            vdd_target, node_name, v2user, timemlstr);

          } /* end while fetch p2 */

        repos::close(&p2);

      } /* end if reg_count */

  } /* End migrate_one_event */