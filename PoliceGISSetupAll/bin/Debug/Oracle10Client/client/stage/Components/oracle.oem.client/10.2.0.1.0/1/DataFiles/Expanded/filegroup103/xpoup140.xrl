/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       xpoup140.xrl
*
*   DESCRIPTION
*
*       Upgrade script for Oracle Expert
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
xp_int_t s1, s2, id, id2, len, parent, id3;


/*******************************************************************************
**  xp_sql_plan table
*******************************************************************************/

repos::execute("create table xp_sql_plan "
               "("
               "statement_id        varchar2(30),"
               "timestamp           date,"
               "remarks             varchar2(80),"
               "operation           varchar2(30),"
               "options             varchar2(30),"
               "object_node         varchar2(128),"
               "object_owner        varchar2(30),"
               "object_name         varchar2(30),"
               "object_instance     numeric,"
               "object_type         varchar2(30),"
               "optimizer           varchar2(255),"
               "search_columns      numeric,"
               "id                  numeric,"
               "parent_id           numeric,"
               "position            numeric,"
               "cost                numeric,"
               "cardinality         numeric,"
               "bytes               numeric,"
               "other_tag           varchar2(255),"
               "other               long"
               ")");


repos::work_in_progress();

/*******************************************************************************
**  Convert xp_sql
*******************************************************************************/

repos::execute("alter table xp_sql add xp_text long");

s1 = repos::open ("SELECT XP_SQL_ID, XP_STATEMENT, XP_LENGTH FROM XP_SQL");

while (repos::fetch (&s1, &id, &buf, &len))
  {
    if (len > 2000)
      {
        s2 = repos::open ("SELECT XP_SEGMENT FROM XP_SQL_SEG "
                          "WHERE XP_SQL_ID = :1 "
                          "ORDER BY XP_SQL_ID,XP_SEGMENT_ID", &id);

        while (repos::fetch (&s2, buf1))
          {
            strcat(buf, buf1);
          }

        repos::close(&s2);
      }

    repos::execute("UPDATE XP_SQL SET XP_TEXT = :2 WHERE XP_SQL_ID = :1",
                   &id, buf);
  }

repos::close(&s1);

repos::execute_ignore("drop table xp_sql_seg");

repos::work_in_progress();

/*******************************************************************************
**  xp_sql_table
*******************************************************************************/

repos::execute("create table xp_sql_table"
               "("
               "xp_database         number,"
               "xp_table            number,"
               "xp_sql_id           number,"
               "xp_verb             number,"
               "xp_analysis_id      number,"
               "xp_ref_id           number"
               ")");

repos::work_in_progress();

/*******************************************************************************
**  Stats table
*******************************************************************************/

repos::execute("create table xp_work_stats"
               "("
               "xp_id            number,"
               "xp_num_stat_01   number,"
               "xp_num_stat_02   number,"
               "xp_num_stat_03   number,"
               "xp_num_stat_04   number,"
               "xp_num_stat_05   number,"
               "xp_num_stat_06   number,"
               "xp_num_stat_07   number,"
               "xp_num_stat_08   number,"
               "xp_num_stat_09   number,"
               "xp_num_stat_10   number,"
               "xp_num_stat_11   number,"
               "xp_num_stat_12   number,"
               "xp_num_stat_13   number,"
               "xp_num_stat_14   number,"
               "xp_num_stat_15   number,"
               "xp_num_stat_16   number,"
               "xp_num_stat_17   number,"
               "xp_num_stat_18   number,"
               "xp_num_stat_19   number,"
               "xp_num_stat_20   number,"
               "xp_num_stat_21   number,"
               "xp_num_stat_22   number,"
               "xp_num_stat_23   number,"
               "xp_num_stat_24   number,"
               "xp_num_stat_25   number,"
               "xp_num_stat_26   number,"
               "xp_num_stat_27   number,"
               "xp_num_stat_28   number,"
               "xp_num_stat_29   number,"
               "xp_num_stat_30   number,"
               "xp_num_stat_31   number,"
               "xp_num_stat_32   number,"
               "xp_num_stat_33   number,"
               "xp_num_stat_34   number,"
               "xp_num_stat_35   number,"
               "xp_num_stat_36   number,"
               "xp_num_stat_37   number,"
               "xp_num_stat_38   number,"
               "xp_num_stat_39   number,"
               "xp_num_stat_40   number,"
               "xp_date_stat_01  date,"
               "xp_date_stat_02  date,"
               "xp_str_stat_01   varchar(100),"
               "xp_str_stat_02   varchar(100),"
               "xp_str_stat_03   varchar(100),"
               "xp_str_stat_04   varchar(100),"
               "xp_str_stat_05   varchar(100)"
               ")");


repos::execute("insert into xp_work_stats ("
               "xp_id,"
               "xp_num_stat_01,"
               "xp_num_stat_02,"
               "xp_num_stat_03,"
               "xp_num_stat_04,"
               "xp_num_stat_05,"
               "xp_num_stat_06,"
               "xp_num_stat_07,"
               "xp_num_stat_08,"
               "xp_num_stat_09,"
               "xp_num_stat_10,"
               "xp_num_stat_11,"
               "xp_num_stat_12,"
               "xp_num_stat_13,"
               "xp_num_stat_14,"
               "xp_num_stat_15,"
               "xp_num_stat_16,"
               "xp_num_stat_17,"
               "xp_num_stat_18,"
               "xp_num_stat_19,"
               "xp_num_stat_20,"
               "xp_num_stat_21,"
               "xp_num_stat_22,"
               "xp_num_stat_23,"
               "xp_num_stat_24,"
               "xp_num_stat_25,"
               "xp_num_stat_26,"
               "xp_num_stat_27,"
               "xp_num_stat_28,"
               "xp_num_stat_29,"
               "xp_num_stat_30,"
               "xp_num_stat_31,"
               "xp_num_stat_32,"
               "xp_num_stat_33,"
               "xp_num_stat_34,"
               "xp_num_stat_35,"
               "xp_num_stat_36,"
               "xp_num_stat_37,"
               "xp_num_stat_38,"
               "xp_num_stat_39,"
               "xp_num_stat_40,"
               "xp_date_stat_01,"
               "xp_date_stat_02,"
               "xp_str_stat_01,"
               "xp_str_stat_02,"
               "xp_str_stat_03,"
               "xp_str_stat_04,"
               "xp_str_stat_05"
               ") "
               "select "
               "xp_id,"
               "xp_cross_fac_4,"
               "xp_session_index,"
               "xp_session_serial,"
               "xp_nano_start_time,"
               "xp_nano_end_time,"
               "xp_uga_memory,"
               "xp_pga_memory,"
               "xp_db_block_change,"
               "xp_db_block_gets,"
               "xp_consistent_gets,"
               "xp_physical_reads,"
               "xp_redo_entries,"
               "xp_redo_size,"
               "xp_t_scan_rows_got,"
               "xp_sort_memory,"
               "xp_sort_disk,"
               "xp_sort_rows,"
               "xp_cpu_session,"
               "xp_elapsed_session,"
               "xp_cursor_number,"
               "xp_ucpu,"
               "xp_scpu,"
               "xp_input_io,"
               "xp_output_io,"
               "xp_max_rssize,"
               "xp_depth,"
               "xp_missed,"
               "xp_row_count,"
               "xp_pagefaults,"
               "xp_pagefaults_io,"
               "xp_elapsed,"
               "0,0,0,0,0,0,0,0,0,"
               "SYSDATE,SYSDATE,"
               "'','','','','' "
               "from xp_workload_stats");
               
repos::work_in_progress();

/*******************************************************************************
**  SQL entity table
*******************************************************************************/

repos::execute("create table xp_sql_entity"
               "("
               "xp_tune       number,"
               "xp_database   number,"
               "xp_sql_id     number,"
               "xp_id         number,"
               "xp_parent     number,"
               "xp_type       number,"
               "xp_line       number,"
               "xp_column     number,"
               "xp_endline    number,"
               "xp_endcolumn  number,"
               "xp_position   number,"
               "xp_name       varchar2(80),"
               "xp_typename   varchar2(30),"
               "xp_int_01     number,"
               "xp_int_02     number,"
               "xp_int_03     number,"
               "xp_int_04     number,"
               "xp_int_05     number,"
               "xp_int_06     number,"
               "xp_int_07     number,"
               "xp_int_08     number,"
               "xp_int_09     number,"
               "xp_int_10     number,"
               "xp_int_11     number,"
               "xp_int_12     number,"
               "xp_int_13     number,"
               "xp_int_14     number,"
               "xp_int_15     number,"
               "xp_int_16     number,"
               "xp_int_17     number,"
               "xp_int_18     number,"
               "xp_int_19     number,"
               "xp_int_20     number,"
               "xp_int_21     number,"
               "xp_int_22     number,"
               "xp_int_23     number,"
               "xp_int_24     number,"
               "xp_int_25     number,"
               "xp_int_26     number,"
               "xp_int_27     number,"
               "xp_int_28     number,"
               "xp_int_29     number,"
               "xp_int_30     number,"
               "xp_flt_01     number,"
               "xp_flt_02     number,"
               "xp_flt_03     number,"
               "xp_flt_04     number,"
               "xp_flt_05     number,"
               "xp_flt_06     number,"
               "xp_flt_07     number,"
               "xp_flt_08     number,"
               "xp_flt_09     number,"
               "xp_flt_10     number,"
               "xp_str_01     varchar2(80),"
               "xp_str_02     varchar2(80),"
               "xp_str_03     varchar2(80),"
               "xp_str_04     varchar2(80),"
               "xp_str_05     long"
               ")");

repos::work_in_progress();

/*******************************************************************************
** XP_DATAFILE_STATS and XP_DATAFILE_STATS_BEGIN tables and their indexes
*******************************************************************************/

repos::execute("create table xp_datafile_stats"
               "("
               "xp_datafile           number,"
               "xp_database           number,"
               "xp_file_id            number,"
               "xp_filename           varchar2(514),"
               "xp_tablespace_name    varchar2(60),"
               "xp_blocks             number,"
               "xp_used_blk           number,"
               "xp_dirty_blk          number,"
               "xp_phys_blk_wrt       number,"
               "xp_collect_date       date"
               ")");

repos::execute("create table xp_datafile_stats_begin"
               "("
               "xp_datafile           number,"
               "xp_database           number,"
               "xp_file_id            number,"
               "xp_filename           varchar2(514),"
               "xp_tablespace_name    varchar2(60),"
               "xp_blocks             number,"
               "xp_used_blk           number,"
               "xp_dirty_blk          number,"
               "xp_phys_blk_wrt       number"
               ")");

repos::work_in_progress();

/*******************************************************************************
**  xp_validate table changes
**
**  Add new xp_priority column
**  Set current value for existing rows to default of 0
**  Adjust the index by dropping and recreating with new columns
*******************************************************************************/

repos::execute("alter table xp_validate add xp_priority number");
repos::execute("update xp_validate set xp_priority = 0");
repos::execute("commit");

repos::work_in_progress();


/*******************************************************************************
** Table xp_object
**
** Environment object changes
**
*******************************************************************************/

/*******************************************************************************
** For each tune session, create an environment object
*******************************************************************************/

s1 = repos::open ("select xp_id from xp_object where xp_type=10");

while (success == repos::fetch (&s1, &id))
  {

    /***************************************************************************
    ** Get the parent for the tune session
    ***************************************************************************/

    parent = repos::integer ("select xp_parent from xp_object where xp_id = :1",
                             &id);

    /***************************************************************************
    ** Get the service name
    ***************************************************************************/

    repos::select("select xp_name from xp_object where "
                  "xp_id = :1", 1, 1, &parent, &buf);

    /***************************************************************************
    ** Insert the new environment record
    ***************************************************************************/

    id2 = repos::integer ("select xp_rep_control_sequence.nextval "
                             "from xp_rep_control");

    repos::execute 
       ("insert into xp_object "
           "(xp_id,xp_parent,xp_type,xp_name,"
              "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
              "xp_new_object,"
              "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
              "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
              "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
              "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
              "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
              "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
            "values "
              "(:1,:2,600,:3,sysdate,sysdate,0,"
              "2048,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"
              "0,0,0,sysdate,'','','','','','','')",
               &id2, &id, &buf);

    /***************************************************************************
    ** Set the parent for system and device objects to be the environment
    ****************************************************************************/

    repos::execute("update xp_object set xp_parent = :1 "
                      "where xp_parent = :2 and xp_type = 601",
                   &id2, &id);

    repos::execute("update xp_object set xp_parent = :1 "
                      "where xp_parent = :2 and xp_type = 602",
                   &id2, &id);

  }

repos::close (&s1);

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  xp_table
*******************************************************************************/

repos::execute("alter table xp_table add xp_num_12 number "
               "add xp_num_13 number add xp_num_14 number "
               "add xp_num_15 number");

repos::execute("update xp_table set xp_num_12 = 0, xp_num_13 = 0, "
               "xp_num_14 = 0, xp_num_15 = 0");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
** Initialize new database attributes for v$type_size values to missing
*******************************************************************************/

repos::execute ("update xp_object set "
                "xp_num_attr_08 = :1, xp_num_attr_09 = :1, xp_num_attr_10 = :1, "
                "xp_num_attr_11 = :1, xp_num_attr_12 = :1 "
                "where xp_type = 100", &missing);

repos::commit();

repos::work_in_progress();

/*******************************************************************************
** For each tune session, add ops and os instance groups
*******************************************************************************/

s1 = repos::open ("select xp_id from xp_object where xp_type=10");

repos::symbol(304, &buf);
repos::symbol(306, &buf1);

while (success == repos::fetch (&s1, &id))
  {

    /***************************************************************************
    ** Insert the new os and ops records 304-ops 306-os
    ***************************************************************************/

    id2 = repos::integer ("select xp_rep_control_sequence.nextval "
                             "from xp_rep_control");

    repos::execute 
       ("insert into xp_object "
           "(xp_id,xp_parent,xp_type,xp_name,"
              "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
              "xp_new_object,"
              "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
              "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
              "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
              "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
              "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
              "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
            "values "
              "(:1,:2,304,:3,sysdate,sysdate,0,"
              "6114,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"
              "0,0,0,sysdate,'','','','','','','')",
               &id2, &id, &buf);

    repos::execute 
       ("insert into xp_object "
           "(xp_id,xp_parent,xp_type,xp_name,"
              "xp_cre_date,xp_mod_date,xp_position,xp_status,xp_hidden,"
              "xp_new_object,"
              "xp_num_attr_01,xp_num_attr_02,xp_num_attr_03,xp_num_attr_04,"
              "xp_num_attr_05,xp_num_attr_06,xp_num_attr_07,xp_num_attr_08,"
              "xp_num_attr_09,xp_num_attr_10,xp_num_attr_11,xp_num_attr_12,"
              "xp_num_attr_13,xp_num_attr_14,xp_num_attr_15,xp_date_attr_01,"
              "xp_str_attr_01,xp_str_attr_02,xp_str_attr_03,"
              "xp_str_attr_04,xp_str_attr_05,xp_str_attr_06,xp_str_attr_07) "
            "values "
              "(:1,:2,306,:3,sysdate,sysdate,0,"
              "6114,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"
              "0,0,0,sysdate,'','','','','','','')",
               &id2, &id, &buf1);
  }

repos::close (&s1);

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  xp_sql_index
*******************************************************************************/

repos::execute_ignore("drop table xp_sql_index");

repos::execute("create table xp_sql_index "
               "("
               "xp_database             number,"
               "xp_analysis             number,"
               "xp_sql_id               number,"
               "xp_tableref             number,"
               "xp_index                number"
               ")");

repos::work_in_progress();

/*******************************************************************************
**  xp_analysis
*******************************************************************************/

repos::execute("alter table xp_analysis add xp_rank number");

/*******************************************************************************
**  Misc sql tables
*******************************************************************************/

repos::execute_ignore ("drop table xp_sql_context");
repos::execute_ignore ("drop table xp_sql_expression");
repos::execute_ignore ("drop table xp_sql_hint");
repos::execute_ignore ("drop table xp_sql_group_by_column");
repos::execute_ignore ("drop table xp_sql_order_by_column");
repos::execute_ignore ("drop table xp_sql_select_column");
repos::execute_ignore ("drop table xp_sql_update_column");
repos::execute_ignore ("drop table xp_sql_where_column");

/*******************************************************************************
** Table xp_object - Update segment types.  Previously, xp_num_attr_14 
** contained 1 or 0 indicating whether or not a rollback segment was PUBLIC.
** This information could be gotten other ways and since we needed a numeric
** attribute in V1.4 to hold a new Flags() member, set xp_num_attr_14 to
** 0 for V1.4.
*******************************************************************************/

repos::execute("update xp_object set xp_num_attr_14 = 0 where xp_type = 501");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  xp_log_table
*******************************************************************************/

repos::execute("create table xp_log_table "
  "("
    "xp_session              number,"
    "xp_timestamp            date,"
    "xp_checkpoint           date,"
    "xp_text                 long"
  ")");

repos::work_in_progress();

/*******************************************************************************
** Table xp_rep_control - make sure this is the last thing in the file that is
** done
*******************************************************************************/

repos::execute ("update xp_rep_control "
                "set xp_prod_version = :1, xp_repos_version = 411,"
                "xp_rules_loaded = 0",
                current_version);

repos::commit();

repos::work_in_progress();

