/* Copyright (c) Oracle Corporation, 1998, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*      epcmigr.xrl
*
*   DESCRIPTION
*
*      Migrate Trace and Data Viewer repository V1.x to V2.0
*
*   NOTES
*
*******************************************************************************/

  printf(get_text(xprc_gl_wip_mig_epc_head));

/***************************************************************************
 **  Error out if a V1.x repository has already been migrated to the
 **  selected V2 user (found in v2user global)
 ***************************************************************************/

    xp_int_t found;

    target::select("select count (*) from epc_cli_rep_users "
                   "where rep_user_name = LOWER(:1)", 1, 1, &v2user, &found);

    if (found != 0)
        {
          printf(get_text(xprc_gl_wip_mig_epc_info1), v2user);
          return;
        }

/***************************************************************************
 **  Update the Trace repository username since it may have changed during
 **  migration.
 ***************************************************************************/
    
    xp_str_t v1_username;
    xp_int_t v1_user_id, v2_user_id;

    repos::select("select rep_user_name, rep_user_id from epc_cli_rep_users",
                   0, 2, v1_username, v1_user_id);

    /**************************************************************************
    **  Get the next user_id sequence number from the target repos.
    **************************************************************************/

    v2_user_id = target::integer ("select epc_cli_rep_users_sequence.nextval "
                                  "from epc_cli_rep_users");

    target::execute ("insert into epc_cli_rep_users (rep_user_name, rep_user_id) "
                     "values (LOWER(:1), :2)", &v2user, &v2_user_id);

/***************************************************************************
 **  Migrate the EPC_CLI_JOB table
 ***************************************************************************/

    xp_int_t cursor_01, job_id, job_coll_id, job_typ, job_stat;
    xp_str_t job_input_fn;

    cursor_01 = repos::open("select job_id, job_collection_id, "
                                   "job_input_filename, job_type, job_status "
                            "from epc_cli_job");

    while (repos::fetch(&cursor_01, &job_id, &job_coll_id, 
                        &job_input_fn, &job_typ, &job_stat))
    {
        target::execute ("insert into epc_cli_job (rep_user_id, job_id, "
                                 "job_collection_id, job_input_filename, job_type, "
                                 "job_status) "
                         "values (:1, :2, :3, :4, :5, :6)", 
                                  &v2_user_id, &job_id, &job_coll_id, &job_input_fn,
                                  &job_typ, &job_stat);
    }

    repos::close (&cursor_01);

    xp_int_t max_dst_job_id, max_src_job_id, max_job_id;

    max_dst_job_id = target::integer("select epc_cli_job_sequence.nextval from epc_cli_job");
    max_src_job_id =  repos::integer("select epc_cli_job_sequence.nextval from epc_cli_job");

    max_job_id = max(max_dst_job_id, max_src_job_id);
    max_job_id++;

    target::execute("drop sequence epc_cli_job_sequence");

    target::execute(concat("create sequence epc_cli_job_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_job_id));

/***************************************************************************
 **  Migrate the EPC_CLI_NODE table
 ***************************************************************************/

    xp_int_t cursor_02, node_id, node_stat, node_active;
    xp_str_t node_name, last_disc, node_stat_txt;
    
    cursor_02 = repos::open("select node_id, node_name, last_discovered,"
                                 "node_status, node_status_text, node_active "
                            "from epc_cli_node");

    while (repos::fetch(&cursor_02, &node_id, &node_name, &last_disc, 
                        &node_stat, &node_stat_txt, &node_active))
    {
        node_name = repos::gethostname(node_name);
        target::execute ("insert into epc_cli_node (rep_user_id, node_id, "
                                 "node_name, last_discovered, node_status, "
                                 "node_status_text, node_active) "
                         "values (:1, :2, :3, :4, :5, :6, :7)",
                                  &v2_user_id, &node_id, &node_name, &last_disc,
                                  &node_stat, &node_stat_txt, &node_active);
    }

    repos::close (&cursor_02);

    xp_int_t max_dst_node_id, max_src_node_id, max_node_id;

    max_dst_node_id = target::integer("select epc_cli_node_sequence.nextval from epc_cli_node");
    max_src_node_id =  repos::integer("select epc_cli_node_sequence.nextval from epc_cli_node");

    max_node_id = max(max_dst_node_id, max_src_node_id);
    max_node_id++;

    target::execute("drop sequence epc_cli_node_sequence");

    target::execute(concat("create sequence epc_cli_node_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_node_id));

/***************************************************************************
 **  Migrate the EPC_CLI_ENVIRONMENT table
 ***************************************************************************/

    xp_int_t cursor_03, ev_id, ev_node_id, ev_status, ev_active;
    xp_str_t o_home;

    cursor_03 = repos::open("select environment_id, oracle_home, node_id, "
                                 "environment_status, environment_active "
                            "from epc_cli_environment");

    while (repos::fetch(&cursor_03, &ev_id, &o_home, &ev_node_id, 
                        &ev_status, &ev_active))
    {
        target::execute ("insert into epc_cli_environment (rep_user_id, "
                                 "environment_id, oracle_home, node_id, "
                                 "environment_status, environment_active) "
                         "values (:1, :2, :3, :4, :5, :6)",
                                  &v2_user_id, &ev_id, &o_home, &ev_node_id, 
                                  &ev_status, &ev_active);
    }

    repos::close (&cursor_03);

    xp_int_t max_dst_ev_id, max_src_ev_id, max_ev_id;

    max_dst_ev_id = target::integer("select epc_cli_env_sequence.nextval from epc_cli_environment");
    max_src_ev_id =  repos::integer("select epc_cli_env_sequence.nextval from epc_cli_environment");

    max_ev_id = max(max_dst_ev_id, max_src_ev_id);
    max_ev_id++;

    target::execute("drop sequence epc_cli_env_sequence");

    target::execute(concat("create sequence epc_cli_env_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_ev_id));

/***************************************************************************
 **  Migrate the EPC_CLI_SERVICE table
 ***************************************************************************/

    xp_int_t cursor_04, svc_id, svc_env_id, svc_status, svc_active;
    xp_str_t svc_name,  svc_sid;

    cursor_04 = repos::open("select service_id, service_name, environment_id, "
                                 "service_sid, service_status, service_active "
                            "from epc_cli_service");


    while (repos::fetch(&cursor_04, &svc_id, &svc_name, &svc_env_id, &svc_sid, 
                        &svc_status, &svc_active))
    {
        target::execute ("insert into epc_cli_service (rep_user_id, service_id, "
                                 "service_name, environment_id, service_sid, "
                                 "service_status, service_active) "
                         "values (:1, :2, :3, :4, :5, :6, :7)",
                                  &v2_user_id, &svc_id, &svc_name, &svc_env_id, &svc_sid, 
                                  &svc_status, &svc_active);
    }

    repos::close (&cursor_04);

    xp_int_t max_dst_svc_id, max_src_svc_id, max_svc_id;

    max_dst_svc_id = target::integer("select epc_cli_svc_sequence.nextval from epc_cli_service");
    max_src_svc_id =  repos::integer("select epc_cli_svc_sequence.nextval from epc_cli_service");

    max_svc_id = max(max_dst_svc_id, max_src_svc_id);
    max_svc_id++;

    target::execute("drop sequence epc_cli_svc_sequence");

    target::execute(concat("create sequence epc_cli_svc_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_svc_id));

/***************************************************************************
 **  Migrate the EPC_CLI_FDF_FILE table
 ***************************************************************************/

    xp_int_t cursor_05, file_id, fmt_env_id, file_status, file_mod, fac_num, active;
    xp_str_t product_name, ev_set, file_desc, filename, fac_ver, vendor;

    cursor_05 = repos::open("select fdf_file_id, product_name, event_set_name, "
                                 "environment_id, fdf_file_desc, fdf_file_status, "
                                 "fdf_file_modifiable, fdf_file_filename, "
                                 "fdf_file_fac_ver, fdf_file_fac_num, fdf_file_fac_vendor, "
                                 "fdf_file_active "
                            "from epc_cli_fdf_file");

    while (repos::fetch(&cursor_05, &file_id, &product_name, &ev_set, 
                        &fmt_env_id, &file_desc, &file_status, &file_mod, 
                        &filename, &fac_ver, &fac_num, &vendor, &active))
    {
        target::execute ("insert into epc_cli_fdf_file (rep_user_id, fdf_file_id, "
                                 "product_name, event_set_name, "
                                 "environment_id, fdf_file_desc, fdf_file_status, "
                                 "fdf_file_modifiable, fdf_file_filename, "
                                 "fdf_file_fac_ver, fdf_file_fac_num, fdf_file_fac_vendor, "
                                 "fdf_file_active) "
                         "values (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13)",
                                  &v2_user_id, &file_id, &product_name, &ev_set,
                                  &fmt_env_id, &file_desc, &file_status, &file_mod,
                                  &filename, &fac_ver, &fac_num, &vendor, &active);
    }

    repos::close (&cursor_05);

    xp_int_t max_dst_fdf_id, max_src_fdf_id, max_fdf_id;

    max_dst_fdf_id = target::integer("select epc_cli_fdf_sequence.nextval from epc_cli_fdf_file");
    max_src_fdf_id =  repos::integer("select epc_cli_fdf_sequence.nextval from epc_cli_fdf_file");

    max_fdf_id = max(max_dst_fdf_id, max_src_fdf_id);
    max_fdf_id++;

    target::execute("drop sequence epc_cli_fdf_sequence");


    target::execute(concat("create sequence epc_cli_fdf_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_fdf_id));


    xp_int_t cursor_06, coll_id, col_env_id, coll_stat, duration, res_fm, 
             form_flags, commit, part_form, auto_id;
    raw form_pswd;
    xp_str_t coll_name, coll_desc, sched_str, res_fn, form_user, form_srv;

    cursor_06 = repos::open("select collection_id, collection_name, "
                                 "environment_id, collection_status, collection_desc, "
                                 "schedule_string, duration_seconds, results_filename, "
                                 "results_filemax, format_username, format_password, "
                                 "format_service, format_flags, commit_interval, "
                                 "partial_format, node_name, autoformat_id "
                             "from epc_cli_collection");


    while (repos::fetch(&cursor_06, &coll_id, &coll_name, &col_env_id, 
                        &coll_stat, &coll_desc, &sched_str, &duration, 
                        &res_fn, &res_fm, &form_user, &form_pswd, &form_srv,
                        &form_flags, &commit, &part_form, &node_name, &auto_id))
    {
        node_name = repos::gethostname(node_name);
        target::execute ("insert into epc_cli_collection (rep_user_id, "
                                 "collection_id, collection_name, "
                                 "environment_id, collection_status, collection_desc, "
                                 "schedule_string, duration_seconds, results_filename, "
                                 "results_filemax, format_username, format_password, "
                                 "format_service, format_flags, commit_interval, "
                                 "partial_format, node_name, autoformat_id) "
                         "values (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13, :14, :15, :16, :17, :18)",
                                  &v2_user_id, &coll_id, &coll_name, &col_env_id, 
                                  &coll_stat, &coll_desc, &sched_str, &duration, 
                                  &res_fn, &res_fm, &form_user, &form_pswd, &form_srv, 
                                  &form_flags, &commit, &part_form, &node_name, &auto_id);
    }

    repos::close (&cursor_06);

    xp_int_t max_dst_col_id, max_src_col_id, max_col_id;

    max_dst_col_id = target::integer("select epc_cli_col_sequence.nextval from epc_cli_collection");
    max_src_col_id =  repos::integer("select epc_cli_col_sequence.nextval from epc_cli_collection");

    max_col_id = max(max_dst_col_id, max_src_col_id);
    max_col_id++;

    target::execute("drop sequence epc_cli_col_sequence");

    target::execute(concat("create sequence epc_cli_col_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_col_id));
    
/***************************************************************************
 **  Migrate the EPC_CLI_PROGRESS table
 ***************************************************************************/

    xp_int_t cursor_07, prog_id, prog_coll_id, prog_stat, prog_type;
    xp_str_t prog_time, prog_text ;

    cursor_07 = repos::open("select progress_id, collection_id, "
                                 "progress_status, progress_type, progress_time, "
                                 "progress_text "
                            "from epc_cli_progress");



    while (repos::fetch(&cursor_07, &prog_id, &prog_coll_id, &prog_stat, 
                        &prog_type, &prog_time, &prog_text))
    {
        target::execute ("insert into epc_cli_progress (rep_user_id, "
                                 "progress_id, collection_id, "
                                 "progress_status, progress_type, progress_time, "
                                 "progress_text) "
                         "values (:1, :2, :3, :4, :5, :6, :7)",
                                 &v2_user_id, &prog_id, &prog_coll_id, &prog_stat, 
                                 &prog_type, &prog_time, &prog_text);
    }

    
    repos::close (&cursor_07);

    xp_int_t max_dst_prog_id, max_src_prog_id, max_prog_id;

    max_dst_prog_id = target::integer("select epc_cli_progr_sequence.nextval from epc_cli_progress");
    max_src_prog_id =  repos::integer("select epc_cli_progr_sequence.nextval from epc_cli_progress");

    max_prog_id = max(max_dst_prog_id, max_src_prog_id);
    max_prog_id++;

    target::execute("drop sequence epc_cli_progr_sequence");

    target::execute(concat("create sequence epc_cli_progr_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_prog_id));

    xp_int_t cursor_08, fmt_id, real_col_id, part_fmt, fmt_status;
    raw fmt_pswd;
    xp_str_t fmt_time, fmt_user, fmt_service, fmt_col_id;

    
    cursor_08 = repos::open("select format_id, collection_id, format_time, "
                                 "format_username, format_password, format_service, "
                                 "partial_format, format_status, format_col_id "
                            "from epc_cli_format");



    while (repos::fetch(&cursor_08, &fmt_id, &real_col_id, &fmt_time,
                        &fmt_user, &fmt_pswd, &fmt_service, &part_fmt, 
                        &fmt_status, &fmt_col_id))
    {
        target::execute ("insert into epc_cli_format (rep_user_id, "
                                 "format_id, collection_id, format_time, "
                                 "format_username, format_password, format_service, "
                                 "partial_format, format_status, format_col_id) "
                         "values (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10)",
                                 &v2_user_id, &fmt_id, &real_col_id, &fmt_time, 
                                 &fmt_user, &fmt_pswd, &fmt_service, &part_fmt, 
                                 &fmt_status, &fmt_col_id);
    }

    repos::close (&cursor_08);

    xp_int_t max_dst_fmt_id, max_src_fmt_id, max_fmt_id;

    max_dst_fmt_id = target::integer("select epc_cli_fmt_sequence.nextval from epc_cli_format");
    max_src_fmt_id =  repos::integer("select epc_cli_fmt_sequence.nextval from epc_cli_format");

    max_fmt_id = max(max_dst_fmt_id, max_src_fmt_id);
    max_fmt_id++;

    target::execute("drop sequence epc_cli_fmt_sequence");

    target::execute(concat("create sequence epc_cli_fmt_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_fmt_id));

/***************************************************************************
 **  Migrate the EPC_CLI_USAGE table
 ***************************************************************************/

    xp_int_t cursor_09, usage_coll_id;
    xp_str_t prod_name, event_set_name;

    cursor_09 = repos::open("select collection_id, product_name, event_set_name "
                            "from epc_cli_usage");


    while (repos::fetch(&cursor_09, &usage_coll_id, &prod_name, 
                        &event_set_name))
    {
        target::execute ("insert into epc_cli_usage (rep_user_id, collection_id, "
                                 "product_name, event_set_name) "
                         "values (:1, :2, :3, :4)",
                                 &v2_user_id, &usage_coll_id, &prod_name,
                                 &event_set_name);
    }

    repos::close (&cursor_09);

/***************************************************************************
 **  Migrate the EPC_CLI_COLLECT_BY_EVENTID table
 ***************************************************************************/

    xp_int_t cursor_10, by_ev_col_id, by_ev_id;
    xp_str_t by_ev_name;

    cursor_10 = repos::open("select collection_id, event_id, event_name "
                            "from epc_cli_collect_by_eventid");


    while (repos::fetch(&cursor_10, &by_ev_col_id, &by_ev_id, 
                        &by_ev_name))
    {
        target::execute ("insert into epc_cli_collect_by_eventid ( "
                                 "rep_user_id, collection_id, event_id, "
                                 "event_name) "
                         "values (:1, :2, :3, :4)",
                                 &v2_user_id, &by_ev_col_id, &by_ev_id, 
                                 &by_ev_name);
    }

    repos::close (&cursor_10);

/***************************************************************************
 **  Migrate the EPC_CLI_COLLECT_BY_USERID table
 ***************************************************************************/

    xp_int_t cursor_11, by_uid_col_id, uid;
    xp_str_t uname;

    cursor_11 = repos::open("select collection_id, user_id, user_name "
                            "from epc_cli_collect_by_userid");

    while (repos::fetch(&cursor_11, &by_uid_col_id, &uid, &uname))
    {
        target::execute ("insert into epc_cli_collect_by_userid (rep_user_id, "
                                 "collection_id, user_id, user_name) "
                         "values (:1, :2, :3, :4)",
                                 &v2_user_id, &by_uid_col_id, &uid, &uname);
    }

    repos::close (&cursor_11);

/***************************************************************************
 **  Update the EPC_CLI_COL_NAME_SEQUENCE
 ***************************************************************************/

    xp_int_t max_dst_col_name_id, max_src_col_name_id, max_col_name_id;

    max_dst_col_name_id = target::integer("select epc_cli_col_name_sequence.nextval from dual");
    max_src_col_name_id =  repos::integer("select epc_cli_col_name_sequence.nextval from dual");

    max_col_name_id = max(max_dst_col_name_id, max_src_col_name_id);
    max_col_name_id++;

    target::execute("drop sequence epc_cli_col_name_sequence");

    target::execute(concat("create sequence epc_cli_col_name_sequence increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_col_name_id));


/***************************************************************************
 **  Create and load the EPC_VIEW tables
 ***************************************************************************/



/***************************************************************************
 **  Migrate the EPC_CLI_VERSION & EPC_TDV_VERSION tables
 ***************************************************************************/


    target::execute ("update epc_cli_version set version_product=2.1");

    target::execute ("update epc_tdv_version set version_product=2.1");



/***************************************************************************
 **
 **  Migrate any user defined data views from the V1.6 epc_view* tables
 **  into the new V2.0.4 repository.  
 **
 **  First need to see if EpcVwUs.sql has been run at on the V2 target 
 **  repos.  If not skip migrating custom views because the custom data 
 **  views will expect a view_cat_id=100 in the epc_view_category table
 **  and it will not be present because it is created in EpcVwUs.sql
 **  proceeding would cause an error when enabling the constraint in 
 **  epc_mview_category_map because the parent key that is being referenced
 **  does not exist.
 **
 ***************************************************************************/

    xp_int_t    CustomCategoryId = 100;
    xp_int_t    custom_dv_count;
    xp_int_t    EpcVwUs_loaded;
    xp_int_t    max_multi_view_id = 0;
    xp_int_t    max_view_id = 0;


    custom_dv_count = repos::integer ("select count(*) from epc_mview_category_map "
                                 "where view_cat_id = :1", CustomCategoryId);

    EpcVwUs_loaded = target::integer ("select count (*) from epc_view_category "
                                 "where  view_cat_id = :1", CustomCategoryId);

    if (custom_dv_count > 0 && EpcVwUs_loaded == 1)
    {
        xp_int_t    cursor_12, cursor_13, cursor_14, cursor_15, cursor_16, cursor_17;
        xp_int_t    multi_view_id;
        xp_int_t    vendor1, facility_number, parent_event_num, state_mask, view_type;
        xp_str_t    min_facility_version, view_name, view_description, full_view_desc;
        xp_int_t    view_id;
        xp_int_t    vendr, fac_number, event_num, sort_item_num, sort_order, num_rows;
        xp_str_t    min_fac_ver, sort_item_name;
        xp_int_t    item_num, item_order;
        xp_str_t    item_name;


        /**************************************************************************
        **  Disable triggers prior to copying user defined data views.
        **  Foreign key constraints were being violated due to the order of 
        **  inserts into various tables.
        **************************************************************************/
   
        /* printf ("Disable constraints"); */

        target::execute("alter table epc_mview_category_map disable CONSTRAINT view_cat_id_2");
        target::execute("alter table epc_mview_category_map disable CONSTRAINT multi_view_id_3");

        target::execute("alter table epc_view_category disable CONSTRAINT view_cat_id");

        target::execute("alter table epc_view_items disable CONSTRAINT view_id_2");

        target::execute("alter table epc_multi_view_map disable CONSTRAINT multi_view_id_2");
        target::execute("alter table epc_multi_view_map disable CONSTRAINT epc_view_id");


        cursor_12 = repos::open ("select multi_view_id from epc_mview_category_map "
                                 "where view_cat_id = :1", CustomCategoryId);

        /* printf("Repos::open  -  select from epc_mview_category_map where view_cat_id = 100"); */

        /**************************************************************************
        **  Loop through "multi_views" that have the Custom folder as parent.
        **  Custom views have a view_cat_id of 100.
        **************************************************************************/
    
        while (repos::fetch (&cursor_12, &multi_view_id))
        {
            /* printf("Repos::fetch -  select from epc_mview_category_map where view_cat_id = 100 -- Multi_view_id = %1", multi_view_id); */

            target::select ("select max(multi_view_id) from epc_multi_views", 0, 1, &max_multi_view_id);

            if (max_multi_view_id < 500)
                max_multi_view_id = 500;
            max_multi_view_id++;

            /* printf ("New max_multi_view_id set to: %1",max_multi_view_id); */

            target::execute ("insert into epc_mview_category_map "
                                "(view_cat_id, multi_view_id) "
                             "values (:1, :2)",
                                  &CustomCategoryId, &max_multi_view_id);

            /* printf("Target::insert - into epc_mview_category_map values (%1, %2)", CustomCategoryId, max_multi_view_id ); */


            /**********************************************************************
            **  Loop through the custom "multi_views" found in the category map
            **  and insert the epc_multi_view's into the target repos.
            ***********************************************************************/


            cursor_13 = repos::open ("select vendor, facility_number, "
                                            "min_facility_version, parent_event_num, "
                                            "view_name, view_description, "
                                            "full_view_desc, state_mask, view_type "
                                     "from epc_multi_views "
                                     "where multi_view_id = :1", multi_view_id);

            /* printf("Repos::open  - select from epc_multi_views where multi_view_id = XXX"); */

            while (repos::fetch (&cursor_13, &vendor1, &facility_number, &min_facility_version, 
                                 &parent_event_num, &view_name, &view_description, 
                                 &full_view_desc, &state_mask, &view_type))
            {
            
                /* printf("Repos::fetch - select from epc_multi_views where multi_view_id = XXX"); */

                target::execute ("insert into epc_multi_views ( "
                                    "multi_view_id, vendor, facility_number, "
                                    "min_facility_version, parent_event_num, "
                                    "view_name, view_description, full_view_desc, "
                                    "state_mask, view_type) "
                                 "values (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10)",
                                          &max_multi_view_id, &vendor1, &facility_number, 
                                          &min_facility_version, &parent_event_num, 
                                          &view_name, &view_description, &full_view_desc, 
                                          &state_mask, &view_type);

                /* printf("Target::insert - into epc_multi_views where multi_view_id = XXX"); */

            
                /******************************************************************
                **  Loop through the custom "multi_view_map" and insert the 
                **  epc_multi_view's into the target "multi_view_map"
                *******************************************************************/
            

                cursor_14 = repos::open ("select epc_view_id from epc_multi_view_map "
                                         "where multi_view_id = :1", multi_view_id);

                /* printf("Repos::open   - select from epc_multi_view_map where mview_id = XXX"); */
    

                while (repos::fetch (&cursor_14, &view_id))
                {

                    /* printf("Repos::fetch  - select from epc_multi_view_map where mview_id = XXX"); */

                    target::select ("select max(view_id) from epc_view", 0, 1, &max_view_id);

                    if (max_view_id < 500)
                        max_view_id = 500;
                    max_view_id++;

                    target::execute ("insert into epc_multi_view_map ( "
                                        "multi_view_id, epc_view_id) "
                                     "values (:1, :2)", &max_multi_view_id, &max_view_id);

                    /* printf("Target::insert - into epc_multi_view_map"); */


                    /**************************************************************
                    **  Loop through epc_view records
                    **************************************************************/
                

                    cursor_15 = repos::open ("select vendor, facility_number, "
                                                "min_facility_version, view_name, "
                                                "event_num, sort_item_num, "
                                                "sort_item_name, sort_order, num_rows "
                                             "from epc_view "
                                             "where view_id = :1", view_id);

                    /* printf("Repos::open   - select from epc_view where view_id = XXX"); */

                    while (repos::fetch(&cursor_15, &vendr, &fac_number, &min_fac_ver,
                                        &view_name, &event_num, &sort_item_num, 
                                        &sort_item_name, &sort_order, &num_rows))
                    {

                        /* printf("Repos::fetch  - select from epc_view where view_id = XXX"); */

                        target::execute ("insert into epc_view ("
                                            "view_id, vendor, facility_number, "
                                            "min_facility_version, view_name, "
                                            "event_num, sort_item_num, sort_item_name, "
                                            "sort_order, num_rows)"
                                         "values (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10)",
                                            &max_view_id, &vendr, &fac_number, &min_fac_ver,
                                            &view_name, &event_num, &sort_item_num, 
                                            &sort_item_name, &sort_order, &num_rows);

                        /* printf("Target::insert - into epc_view--> view_id = %1  view_name = %2", max_view_id, view_name); */


                        /**************************************************************
                        **  Loop through epc_view_item records
                        **************************************************************/


                        cursor_16 = repos::open ("select item_number, item_name, item_order "
                                                 "from epc_view_items "
                                                 "where view_id = :1", view_id);

                        /* printf("Repos::open   - select from epc_view_items where view_id = XXX"); */

                        while (repos::fetch(&cursor_16, &item_num, &item_name, &item_order))
                        {
                            /* printf("Repos::fetch  - select from epc_view where view_id = XXX"); */
                        
                            target::execute ("insert into epc_view_items ( "
                                                "view_id, item_number, item_name, item_order) "
                                             "values (:1, :2, :3, :4)",
                                                &max_view_id, &item_num, &item_name, &item_order);

                            /* printf("Target::insert - into epc_view_items item_name= %1 item_num= %2 item_order= %3", item_num, item_name, item_order); */

                        }
                    }
                }

            }

        }

        /**************************************************************************
        **  Re-enable constraints
        **************************************************************************/

        target::execute("alter table epc_multi_view_map enable CONSTRAINT MULTI_VIEW_ID_2");
        target::execute("alter table epc_multi_view_map enable CONSTRAINT EPC_VIEW_ID");

        target::execute("alter table epc_view_items enable CONSTRAINT VIEW_ID_2");

        target::execute("alter table epc_view_category enable CONSTRAINT VIEW_CAT_ID");

        target::execute("alter table epc_mview_category_map enable CONSTRAINT VIEW_CAT_ID_2");
        target::execute("alter table epc_mview_category_map enable CONSTRAINT MULTI_VIEW_ID_3");

        /* printf("Re-enable constraints"); */

        /**************************************************************************
        **  Close cursors
        ***************************************************************************/

        /* printf("CLOSE CURSORS"); */

        repos::close(cursor_12);
        repos::close(cursor_13);
        repos::close(cursor_14);
        repos::close(cursor_15);
        repos::close(cursor_16);

    }  /*  End of loop to migrate user defined data views. */

    /**************************************************************************
    **  Drop and recreate the epc_view_id sequence.  It needs to have a 
    **  start value of max known view_id+1
    ***************************************************************************/
    
    /* printf("Drop and re-create the epc_view_id sequence"); */
    
    xp_int_t    tmpMax;
    target::select ("select max(view_id) from epc_view", 0, 1, &tmpMax);

    tmpMax++;
    max_view_id++;
    max_view_id = max(tmpMax, max_view_id);

    if (max_view_id < 500)
        max_view_id = 500;

    target::execute("drop sequence epc_view_id");

    target::execute(concat("create sequence epc_view_id increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_view_id));

    /**************************************************************************
    **  Drop and recreate the epc_mview_id sequence.  It needs to have a 
    **  start value of max known multi_view_id+1
    ***************************************************************************/

    /* printf("Drop and re-create the epc_mview_id sequence"); */

    target::select ("select max(multi_view_id) from epc_multi_views", 0, 1, &tmpMax);

    tmpMax++;
    max_multi_view_id++;
    max_multi_view_id = max(tmpMax, max_multi_view_id);

    if (max_multi_view_id < 500)
        max_multi_view_id = 500;

    target::execute("drop sequence epc_mview_id");

    target::execute(concat("create sequence epc_mview_id increment by 1 nominvalue maxvalue 999999999 order cycle cache 50 start with  ", max_multi_view_id));



    /**************************************************************************
    **  Migrate the EPC_VIEW_PREFERENCES table.
    **
    **  NOTE:  This code is commented out for now because of invalid 
    **         column name errors from the source epc_view_preferences
    **         table.  In the case I was testing the show_selects, show_inserts, 
    **         columns were not in the source repository.  They are columns
    **         that were introduces in V2.0.4.  Since epc_view tables were 
    **         never migrated in the past (because behavior would be different
    **         if the tables were in a repos vs. stand alone formatted data.
    **         Upgrades of epc_view data were always done in code...  
    **
    **         Code would have to migrate columns individually (grouped by
    **         epc_view version) as they were introduced to the preferences 
    **         table.
    **
    ***************************************************************************/

/*
    xp_int_t max_rows_retrieved, sort_asc_or_desc, show_welcome, discard_changes_withoutprompt,
             discard_recursive_sql, pfont_height, pfont_weight, pfont_italic, 
             pfont_uline, pfont_pitchfamily, pfont_charset, 
             show_selects, show_inserts, show_updates, show_deletes;
    xp_str_t pfont_facename;

    repos::select ("select max_rows_retrieved, sort_order, show_welcome, "
                       "discard_changes_withoutprompt, discard_recursive_sql, "
                       "pfont_height, pfont_weight, pfont_italic, "
                       "pfont_uline, pfont_pitchfamily, pfont_charset, "
                       "pfont_facename, show_selects, show_inserts, "
                       "show_updates, show_deletes "
                   "from epc_view_preferences", 0, 16, 
                       &max_rows_retrieved, &sort_asc_or_desc, &show_welcome, 
                       &discard_changes_withoutprompt, &discard_recursive_sql, 
                       &pfont_height, &pfont_weight, &pfont_italic, 
                       &pfont_uline, &pfont_pitchfamily, &pfont_charset, 
                       &pfont_facename, &show_selects, &show_inserts, 
                       &show_updates, &show_deletes);

    target::execute ("insert into epc_view_preferences (max_rows_retrieved, sort_order, "
                        "show_welcome, discard_changes_withoutprompt, discard_recursive_sql, "
                        "pfont_height, pfont_weight, pfont_italic, pfont_uline, "
                        "pfont_pitchfamily, pfont_charset, pfont_facename, "
                        "show_selects, show_inserts, show_updates, show_deletes) "
                      "values (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13, :14, :15, :16)",
                         &max_rows_retrieved, &sort_asc_or_desc, &show_welcome, 
                         &discard_changes_withoutprompt, &discard_recursive_sql, 
                         &pfont_height, &pfont_weight, &pfont_italic, 
                         &pfont_uline, &pfont_pitchfamily, &pfont_charset, 
                         &pfont_facename, &show_selects, &show_inserts, 
                         &show_updates, &show_deletes);

     printf ("Migrated the epc_view_preferences table");
*/

    target::commit();

    printf(get_text(xprc_gl_wip_mig_epc_tail));
