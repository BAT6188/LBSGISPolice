/* Copyright (c) Oracle Corporation, 1998.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*      vtmmigr.xrl
*
*   DESCRIPTION
*
*      Migrate Performance Manager / Capacity Planner repository V1.x to V2.0
*
*   NOTES
*
*******************************************************************************/
printf(get_text(xprc_gl_wip_mig_vtm_head));

    /***************************************************************************
    **  Make sure the selected v2 user has not already been migrated from v1 repository
    ***************************************************************************/

    xp_int_t count;
    count = target::integer("select count(*) "
                            "from smp_vtm_chart_defn cd, smp_vtm_udchart_defn ud "
                            "where cd.owner_name = :1 "
                            "and cd.owner_name = ud.owner_name", v2user);

    if (count > 0)
    {
      printf(get_text(xprc_gl_wip_mig_vtm_info1), v2user);
    }
    if (count == 0)
    {
        xp_int_t num_targets, num_recordings, num_udcharts;
        xp_int_t cursor1, ref_id, service_type, new_ref_id;
        xp_int_t cursor2, chart_ref_id, status, interval;
        xp_str_t service_name, chart_name, class_name, cartridge_name, owner_name;
        xp_str_t file_path, recording_name, start_time, end_time, script_text;
        xp_str_t column_seq, column_label, function1;
        xp_str_t column1, operation, function2, column2, key_column;

        /***************************************************************************
        **  Migrate SMP_VTM_CHART_DEFN and its detail table: SMP_VTM_RECORDING_DATA
        ***************************************************************************/

        num_recordings = 0;

        cursor1 = repos::open("select ref_id, service_name, service_type, "
                              "chart_name, class_name, cartridge_name, owner_name "
                              "from smp_vtm_chart_defn");

        while (repos::fetch(&cursor1, &ref_id, &service_name, &service_type,
                                      &chart_name, &class_name, &cartridge_name, &owner_name))
        {
            new_ref_id = target::integer("select smp_vtm_chart_defn_seq.nextval from sys.dual");

            target::execute ("insert into smp_vtm_chart_defn (ref_id, service_name, "
                             "service_type, chart_name, class_name, cartridge_name, owner_name) "
                             "values (:1, :2, decode(:3, 16385, 'oracle_sysman_node',
                                                         16387, 'oracle_sysman_database',
                                                         16388, 'oracle_sysman_rdbdatabase'),
                                      :4, :5, :6, :7)",
                             &new_ref_id, &service_name, &service_type, &chart_name, &class_name,
                             &cartridge_name, &v2user);

            cursor2 = repos::open("select to_char(start_time,'999999999999'), "
                                  "file_path, status, "
                                  "to_char(end_time,'999999999999'), "
                                  "interval, recording_name "
                                  "from smp_vtm_recording_data "
                                  "where chart_ref_id = :1", ref_id);

            while (repos::fetch(&cursor2, &start_time, &file_path, &status,
                                             &end_time, &interval, &recording_name))
            {
                target::execute ("insert into smp_vtm_recording_data (chart_ref_id, start_time, "
                                 "file_path, status, end_time, interval, recording_name) "
                                 "values (:1, :2, :3, :4, :5, :6, :7)",
                                 &new_ref_id, &start_time, &file_path,
                                 &status, &end_time, &interval, &recording_name);
            }

            repos::close (&cursor2);
            num_recordings += 1;
        }

        repos::close (&cursor1);
        target::close (&cursor2);
        target::commit();

        /***************************************************************************
        **  Migrate SMP_VTM_UDCHART_DEFN and its detail table: SMP_VTM_UDCHART_COLUMNS
        ***************************************************************************/

        num_udcharts = 0;

        cursor1 = repos::open("select ref_id, service_name, service_type, "
                              "chart_name, cartridge_name, script_text, owner_name "
                              "from smp_vtm_udchart_defn");

        while (repos::fetch(&cursor1, &ref_id, &service_name, &service_type,
                            &chart_name, &cartridge_name, &script_text, &owner_name))
        {
            new_ref_id = target::integer("select smp_vtm_udchart_defn_seq.nextval from sys.dual");

            target::execute ("insert into smp_vtm_udchart_defn (ref_id, service_name, "
                             "service_type, chart_name, cartridge_name, script_text, owner_name) "
                             "values (:1, :2, decode(:3, 16385, 'oracle_sysman_node',
                                                         16387, 'oracle_sysman_database',
                                                         16388, 'oracle_sysman_rdbdatabase'),
                                      :4, :5, :6, :7)",
                             &new_ref_id, &service_name, &service_type, &chart_name,
                             &cartridge_name, &script_text, &v2user);

            cursor2 = repos::open("select column_seq, column_label, function1, column1, "
                                  "operation, function2, column2 "
                                  "from smp_vtm_udchart_columns "
                                  "where chart_ref_id = :1", &ref_id);

            while (repos::fetch(&cursor2, &column_seq, &column_label, &function1,
                                &column1, &operation, &function2, &column2))
            {
                target::execute ("insert into smp_vtm_udchart_columns (chart_ref_id, column_seq, "
                                 "column_label, function1, column1, operation, function2, column2, key_column) "
                                 "values (:1, :2, :3, :4, :5, :6, :7, :8, 0)",
                                 &new_ref_id, &column_seq, &column_label, &function1, &column1,
                                 &operation, &function2, &column2);
            }

            repos::close (&cursor2);
            num_udcharts += 1;
        }

        repos::close (&cursor1);
        target::close (&cursor2);
        target::commit();
    }

printf(get_text(xprc_gl_wip_mig_vtm_tail));



