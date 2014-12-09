/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       xpoup130.xrl
*
*   DESCRIPTION
*
*       Upgrade script for Oracle Expert
*
*   NOTES
*
*       Upgrades v1.2 repository to V1.3
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_hashid_t hash;
xp_char_t buf, tmp;
xp_int_t s1, s2, id, len, parent;

/*******************************************************************************
**  Create temporary index
*******************************************************************************/

repos::execute_ignore ("create index xp_object_tmp_idx on xp_object "
                       "xp_type, xp_id, xp_parent");

/*******************************************************************************
**  Get rid of xp_ss*
*******************************************************************************/

repos::execute_ignore ("drop table xp_ss");
repos::execute_ignore ("drop sequence xp_ss_sequence");

/*******************************************************************************
** Table xp_instance_params
*******************************************************************************/

repos::execute ("update xp_instance_params set xp_value_recom = :1 "
                "where xp_value_recom = '' or xp_value_recom is null", missing);

repos::work_in_progress();

/*******************************************************************************
** Table xp_instance_stats
*******************************************************************************/

repos::execute("alter table xp_instance_stats add ("
               "xp_total_latch_sleeps number,"
               "xp_sort_latch_sleeps number,"
               "xp_sort_latch_sleeps_ratio number)");

repos::execute("update xp_instance_stats set "
               "xp_total_latch_sleeps = :1,"
               "xp_sort_latch_sleeps = :1,"
               "xp_sort_latch_sleeps_ratio = :1", missing);

repos::commit();

repos::work_in_progress();

/*******************************************************************************
** Table xp_instance_stats_begin
*******************************************************************************/

repos::execute("alter table xp_instance_stats_begin add ("
               "xp_total_latch_sleeps number,"
               "xp_sort_latch_sleeps number)");

repos::execute("update xp_instance_stats_begin set "
               "xp_total_latch_sleeps = :1,"
               "xp_sort_latch_sleeps = :1", missing);

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Table xp_workload_stats
*******************************************************************************/

repos::execute("alter table xp_workload_stats add("
               "xp_cross_fac_4 number,"
               "xp_cpu_session number," 
               "xp_elapsed_session number,"
               "xp_cursor_number number,"
               "xp_ucpu number,"
               "xp_scpu number,"
               "xp_input_io number,"
               "xp_output_io number,"
               "xp_pagefaults number,"
               "xp_pagefaults_io number,"
               "xp_max_rssize number,"
               "xp_depth number,"
               "xp_missed number,"
               "xp_row_count number)");

repos::execute("update xp_workload_stats set "
               "xp_cross_fac_4 = 0,"
               "xp_cpu_session = 0,"
               "xp_elapsed_session = 0,"
               "xp_cursor_number = 0,"
               "xp_ucpu = 0,"
               "xp_scpu = 0,"
               "xp_input_io = 0,"
               "xp_output_io = 0,"
               "xp_pagefaults = 0,"
               "xp_pagefaults_io = 0,"
               "xp_max_rssize = 0,"
               "xp_depth = 0,"
               "xp_missed = 0,"
               "xp_row_count = 0");

repos::commit();

repos::execute("create index xp_req_id_01 on xp_workload_stats (xp_id)");

repos::work_in_progress();

/*******************************************************************************
**  Fix the xp_validate table
*******************************************************************************/

repos::execute("alter table xp_validate add xp_sequence number");

repos::execute("update xp_validate set xp_sequence = xp_details_start");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
** Table xp_sql
*******************************************************************************/

repos::execute("update xp_sql set xp_sql_hash_id = NULL");

repos::execute("alter table xp_sql modify (xp_sql_hash_id raw(16))");

s1 = repos::open ("select xp_sql_id, xp_statement, xp_length from xp_sql");

while (success == repos::fetch (&s1, &id, &buf, &len))
  {
    s2 = repos::open ("select xp_segment from xp_sql_seg "
                      "where xp_sql_id = :1 "
                      "order by xp_sql_id, xp_segment_id", &id);

    while (success == repos::fetch (&s2, tmp))
      {
        strcat (buf, tmp);
      }

    repos::close (&s2);

    repos::compute_hash (buf, &hash);

    repos::execute("update xp_sql set xp_sql_hash_id = :2 "
                   "where xp_sql_id = :1",
                   &id, &hash);
  }

repos::close (&s1);

repos::work_in_progress();

/*******************************************************************************
** Table xp_sql_statement_work
*******************************************************************************/

repos::execute("update xp_sql_statement_work set "
               "xp_sql_hash_id = null, "
               "xp_case_hash_id = null, "
               "xp_spacing_hash_id = null, "
               "xp_variable_hash_id = null, "
               "xp_order_hash_id = null, "
               "xp_format_hash_id = null");

repos::execute("alter table xp_sql_statement_work modify ("
               "xp_sql_hash_id raw(16),"
               "xp_case_hash_id raw(16),"
               "xp_spacing_hash_id raw(16),"
               "xp_variable_hash_id raw(16),"
               "xp_order_hash_id raw(16),"
               "xp_format_hash_id raw(16))");

/*******************************************************************************
**  Convert indexes from being children of lschema to children of owner
**  table.
*******************************************************************************/

repos::execute ("update xp_object o set o.xp_parent = (select i.xp_table from "
                "xp_index i where i.xp_index = o.xp_id) "
                "where o.xp_type = 408");

repos::execute("alter table xp_index add (xp_tmp_01 number, "
               "xp_tmp_02 number, xp_tmp_03 number, "
               "xp_tmp_04 number, xp_tmp_05 number)");

repos::work_in_progress();

/*******************************************************************************
**  Add column to xp_sql_ ...
*******************************************************************************/

repos::execute("alter table xp_sql_index add (xp_sql_id number)");

repos::execute("alter table xp_sql_select_column add "
               "(xp_distinct_flag number)");

repos::execute("alter table xp_sql_expression add "
               "(xp_flags number)");

repos::execute("alter table xp_sql_context add "
               "(xp_flags number,"
               " xp_database number)");

repos::execute("create index xp_sql_context_03 on xp_sql_context "
               "(xp_database, xp_sql_id)");

repos::work_in_progress();

/*******************************************************************************
**  xp_user_rule
*******************************************************************************/

repos::execute("create index xp_user_rule_04 on xp_user_rule "
               "(xp_rule_id)");

repos::work_in_progress();

/*******************************************************************************
** Table xp_cluster
*******************************************************************************/

repos::execute("alter table xp_cluster add xp_str_01 varchar(30)");

repos::work_in_progress();

/*******************************************************************************
** Table xp_table
*******************************************************************************/

repos::execute("alter table xp_table add "
               "(xp_num_11 number, xp_str_01 varchar(30))");

repos::execute("update xp_table set xp_num_11 = 0, xp_num_10 = 0");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
** Table xp_validate
********************************************************************************

repos::execute("alter table xp_validate "
               "add (xp_sequence number)");

repos::work_in_progress();

/*******************************************************************************
** Table xp_validate
*******************************************************************************/

repos::execute("alter table xp_request "
               "add (xp_io_count number(15))");

repos::work_in_progress();

/*******************************************************************************
** Initialize new database attributes for v$type_size values to missing
*******************************************************************************/

repos::execute ("update xp_object set "
                "xp_num_attr_05 = :1, xp_num_attr_06 = :1, xp_num_attr_07 = :1 "
                "where xp_type = 100", &missing);

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Update sizes that we had previously scaled by 2048 to be scaled by
**  1024.  
*******************************************************************************/

repos::execute ("update xp_object set xp_num_attr_02 = xp_num_attr_02 * 2, "
                "xp_num_attr_03 = xp_num_attr_03 * 3 "
                "where xp_type = 507"); 

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Update sizes in datafile objects
*******************************************************************************/

repos::execute ("update xp_object set xp_num_attr_01 = xp_num_attr_01 * 2 "
                "where xp_type = 503");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Update sizes in segment objects
*******************************************************************************/

repos::execute ("update xp_object set xp_num_attr_03 = xp_num_attr_03 * 2 "
                "where xp_type = 501");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Resolve new tune session hierarchy
*******************************************************************************/

xp_int_t id2, id3;
xp_char_t name,attr_02,attr_05,attr_04,attr_06;

s1 = repos::open ("select xp_id from xp_object where xp_type = 10");

while (success == repos::fetch (&s1, &id))
  {
    /***************************************************************************
    **  The next tune session has been fetched.  Get the database and instance
    **  info.
    ***************************************************************************/

    id2 = repos::integer("select xp_id from xp_object where xp_type = 100 "
                         "and xp_parent = :1", &id);

    id3 = repos::integer("select xp_id from xp_object where xp_type = 200 "
                         "and xp_parent = :1", &id2);

    repos::select("select xp_name, xp_str_attr_02, xp_str_attr_05,"
                  "xp_str_attr_04, xp_str_attr_06 from xp_object where "
                  "xp_id = :1", 1, 5, &id3, name, attr_02, attr_05, attr_04, 
                  attr_06);

    /***************************************************************************
    **  Check for a service record.  If not found, create one
    ***************************************************************************/

    parent = repos::integer ("select count(*) from xp_object where "
                             "xp_type = 171 "
                             "and xp_name = :1", attr_04);

    if (!parent)
      {
        parent = repos::integer("select xp_rep_control_sequence.nextval from "
                                "xp_rep_control");

        repos::execute ("insert into "
                 "xp_object(xp_id,xp_parent,xp_type,xp_name,"
                 "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
                 "xp_new_object,"
                 "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
                 "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
                 "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
                 "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
                 "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
                 "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
                 "values (:1,0,171,:2,sysdate,sysdate,0,"
                 "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"
                 "0,0,0,sysdate,'',:3,'',:4,:5,:6,'')",
                 &parent, name, attr_02, attr_04, attr_05, attr_06);
      }

    repos::execute("update xp_object set xp_parent = :2, "
                   "xp_hidden = 0 where xp_id = :1", &id, &parent);

    /***************************************************************************
    **  Create the collection records for the tunesession
    ***************************************************************************/

    id2 = repos::integer ("select xp_rep_control_sequence.nextval "
                          "from xp_rep_control");

    repos::execute ("insert into "
                    "xp_object (xp_id, xp_parent, xp_type, xp_name,"
                     "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
                     "xp_new_object,"
                     "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
                     "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
                     "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
                     "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
                     "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
                     "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
                     "values (:1,:2,9001,'dbcollectopt',sysdate,sysdate,0,"
                     "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"
                     "0,1,1,sysdate,'','','','','','','')",
                     &id2, &id);

    repos::execute("update xp_object set xp_num_attr_08 = :2 where xp_id = :1",
                   &id, &id2);

    id2 = repos::integer ("select xp_rep_control_sequence.nextval "
                          "from xp_rep_control");

    repos::execute ("insert into "
                    "xp_object (xp_id, xp_parent, xp_type, xp_name,"
                     "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
                     "xp_new_object,"
                     "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
                     "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
                     "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
                     "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
                     "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
                     "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
                     "values (:1,:2,9001,'incollectopt',sysdate,sysdate,0,"
                     "0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,"
                     "0,1,1,sysdate,'','','','','','','')",
                     &id2, &id);

    repos::execute("update xp_object set xp_num_attr_09 = :2 where xp_id = :1",
                   &id, &id2);

    id2 = repos::integer ("select xp_rep_control_sequence.nextval "
                          "from xp_rep_control");

    repos::execute ("insert into "
                    "xp_object (xp_id, xp_parent, xp_type, xp_name,"
                     "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
                     "xp_new_object,"
                     "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
                     "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
                     "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
                     "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
                     "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
                     "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
                     "values (:1,:2,9001,'lscollectopt',sysdate,sysdate,0,"
                     "0,0,0,0,0,0,0,0,0,1,1000,0,1,0,0,"
                     "0,1,1,sysdate,'','','','','','','')",
                     &id2, &id);

    repos::execute("update xp_object set xp_num_attr_10 = :2 where xp_id = :1",
                   &id, &id2);

    id2 = repos::integer ("select xp_rep_control_sequence.nextval "
                          "from xp_rep_control");

    repos::execute ("insert into "
                    "xp_object (xp_id, xp_parent, xp_type, xp_name,"
                     "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
                     "xp_new_object,"
                     "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
                     "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
                     "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
                     "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
                     "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
                     "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
                     "values (:1,:2,9001,'envcollectopt',sysdate,sysdate,0,"
                     "0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,"
                     "0,1,0,sysdate,'','','','','','','')",
                     &id2, &id);

    repos::execute("update xp_object set xp_num_attr_11 = :2 where xp_id = :1",
                   &id, &id2);

    id2 = repos::integer ("select xp_rep_control_sequence.nextval "
                          "from xp_rep_control");

    repos::execute ("insert into "
                    "xp_object (xp_id, xp_parent, xp_type, xp_name,"
                     "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
                     "xp_new_object,"
                     "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
                     "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
                     "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
                     "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
                     "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
                     "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
                     "values (:1,:2,9001,'wkcollectopt',sysdate,sysdate,0,"
                     "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"
                     "0,1,1,sysdate,'','','','','','','')",
                     &id2, &id);

    repos::execute("update xp_object set xp_num_attr_12 = :2 where xp_id = :1",
                   &id, &id2);

    repos::commit();
  }

repos::close (&s1);
repos::commit();

repos::work_in_progress();

/*******************************************************************************
** Table xp_rep_control
*******************************************************************************/

repos::execute ("update xp_rep_control "
                "set xp_prod_version = :1, xp_repos_version = 410,"
                "xp_rules_loaded = 0",
                current_version);

repos::commit();

repos::execute_ignore ("drop index xp_object_tmp_idx");

repos::work_in_progress();

/*******************************************************************************
**  Update version
*******************************************************************************/

repos::update_version ("EXPERT");
