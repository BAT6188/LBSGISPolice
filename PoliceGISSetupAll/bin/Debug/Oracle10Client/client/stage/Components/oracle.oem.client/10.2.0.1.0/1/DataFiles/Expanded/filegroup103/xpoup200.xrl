/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       xpoup200.xrl
*
*   DESCRIPTION
*
*       Upgrade script for Oracle Expert
*
*   NOTES
*
*       Upgrades v1.6 repository to V2.0
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t user, pwd, svc;
xp_int_t id;

/*******************************************************************************
**  Tablespace changes.  For each tablespace set:
**    summary info to missing
**    MINIMUM_EXTENT to missing
**    RECOM_MINIMUM_EXTENT to missing
**    EXTENT_MANAGEMENT to XPTS_K_EXTENT_MANAGEMENT_DICTIONARY
**    ALLOCATION_TYPE to XPTS_K_ALLOCATION_USER
*******************************************************************************/

repos::execute("update xp_tablespace set "
               "xp_valid_summary_info=0,"
               "xp_segment_types=0,xp_system_owned_segments=0,"
               "xp_user_default=0,xp_user_temporary=0,"
               "xp_user_is_system=0,"
               "xp_recom_minimum_extent='-999'");

repos::execute("update xp_object set "
               "xp_num_attr_10='-999',xp_num_attr_11=1,"
               "xp_num_attr_12='-999',xp_num_attr_13=2 "
               "where xp_type=500");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Segment changes.  For each segment set:
**    obsolete xp_num_attr_03 (size) info to missing
**    obsolete xp_num_attr_04 (extents) info to missing
**    xp_str_attr_05 (partition_name) to missing instead of previous '' default
*******************************************************************************/


repos::execute("update xp_object set "
               "xp_num_attr_03='-999',xp_num_attr_04='-999' "
               "where xp_type=501");

repos::execute("update xp_object set "
               "xp_str_attr_05='-999' "
               "where xp_type=501 and xp_str_attr_05 is null");

repos::commit();

repos::work_in_progress();


/*******************************************************************************
**  Service changes.  For each service set "Created by" attribute to
**  "discovered".
*******************************************************************************/


repos::execute("update xp_object set xp_num_attr_01=1 "
               "where xp_type=171 or xp_type=172");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
** Instance Stats changes.
*******************************************************************************/

repos::execute("update xp_instance_stats set "
               "xp_sys_undo_header_waits='-999',"
               "xp_sys_undo_block_waits='-999',"
               "xp_undo_header_waits='-999',"
               "xp_undo_block_waits='-999',"
               "xp_log_switch_waits='-999',"
               "xp_sys_undo_header_waits_ratio='-999',"
               "xp_sys_undo_block_waits_ratio='-999',"
               "xp_undo_header_waits_ratio='-999',"
               "xp_undo_block_waits_ratio='-999',"
               "xp_concurrent_trans='-999',"
               "xp_concurrent_parallel_trans='-999',"
               "xp_online_rbs='-999'");

repos::execute("update xp_instance_stats_begin set "
               "xp_sys_undo_header_waits='-999',"
               "xp_sys_undo_block_waits='-999',"
               "xp_undo_header_waits='-999',"
               "xp_undo_block_waits='-999',"
               "xp_log_switch_waits='-999',"
               "xp_concurrent_trans='-999',"
               "xp_concurrent_parallel_trans='-999',"
               "xp_online_rbs='-999'");


repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Update collection objects
*******************************************************************************/

repos::execute("update xp_object set "
               "xp_num_attr_06=0,xp_num_attr_07=0,xp_num_attr_08=0 "
               "where xp_name='DBCOLLOBJECT'");

repos::execute("update xp_object set "
               "xp_num_attr_07=0,xp_num_attr_12=0 "
               "where xp_name='LSCOLLOBJECT'");

repos::execute("update xp_object set "
               "xp_num_attr_05=0,"
               "xp_num_attr_06=0,"
               "xp_num_attr_07=0,"
               "xp_num_attr_08=0,"
               "xp_num_attr_09=1,"
               "xp_num_attr_10=0,"
               "xp_num_attr_11=0,"
               "xp_num_attr_12=0,"
               "xp_str_attr_05=xp_str_attr_03 "
               "where xp_name='WKCOLLOBJECT'");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Convert XP_SQL to new format
*******************************************************************************/

repos::xp_convert_sql();
repos::work_in_progress();


/*******************************************************************************
**  Update xp_parent and xp_sql_hash_id on xp_request table
*******************************************************************************/

raw  sql_hash_id;
xp_int_t  cur1, cur2, request_id, sql_Id, parent_id;
xp_char_t upd_req_parent = "update xp_request set xp_parent = :2 where xp_request = :1";
xp_char_t upd_req_sql_hash_id = "update xp_request set xp_sql_hash_id = :2 where xp_request = :1";

cur1 = repos::open("select xp_sql_id,xp_request from xp_request");

while (repos::fetch(&cur1,&sql_Id,&request_id))
  {
    cur2 = repos::open("SELECT xp_parent "
                     "FROM xp_object "
                     "WHERE xp_id  "
                     "IN (SELECT xp_parent "
                     "    FROM xp_object "
                     "    WHERE xp_id "
                     "    IN (SELECT xp_parent "
                     "        FROM xp_object "
                     "        WHERE xp_id = :1))", request_id);


    while (repos::fetch(&cur2,&parent_id))
      {
        repos::execute(upd_req_parent, &request_id, &parent_id);
      }
    repos::close(&cur2);

    cur2 = repos::open("SELECT xp_sql_hash_id from xp_sql where xp_sql_id = "
                     ":1", sql_Id);
    while (repos::fetch(&cur2,&sql_hash_id))
      {
        repos::execute(upd_req_sql_hash_id, &request_id, &sql_hash_id);
      }
    repos::close(&cur2);
  }

repos::close(&cur1);
repos::commit();
repos::work_in_progress();

/*******************************************************************************
**  Remove programs and transactions from the workload
*******************************************************************************/

xp_int_t s1,s2,s3,s4,app, prog, trans, req;
xp_char_t exec = "SELECT XP_ID FROM XP_OBJECT WHERE XP_PARENT = :1";
xp_char_t upd = "UPDATE XP_OBJECT SET XP_PARENT = :2 WHERE XP_ID = :1";

s1 = repos::open("SELECT XP_ID FROM XP_OBJECT WHERE XP_TYPE = 701");

while (repos::fetch (&s1, &app))
  {
    s2 = repos::open(exec,&app);

    while (repos::fetch(&s2, &prog))
      {
        s3 = repos::open(exec,&prog);
    
        while (repos::fetch(&s3, &trans))
          {
            s4 = repos::open(exec, &trans);
        
            while (repos::fetch(&s4, &req))
              {
                repos::execute(upd, &req, &app);
              }
        
            repos::close(&s4);
          }
    
        repos::close(&s3);
        repos::commit();
      }

    repos::close(&s2);
  }

repos::close(&s1);

repos::execute("DELETE FROM XP_OBJECT WHERE XP_TYPE IN (702,703)");
repos::commit();

repos::execute("DELETE FROM XP_TRANSACTION");
repos::commit();

repos::work_in_progress();

/*******************************************************************************
**  Update sql id 
*******************************************************************************/

xp_int_t sql_id;
xp_float_t freq;
xp_char_t upd_obj = "update xp_object set xp_num_attr_07 = :2 where xp_id = :1";
xp_char_t upd_sql = "update xp_sql set executions = :2 where xp_sql_id = :1";

s1 = repos::open("select xp_sql_id,xp_request,xp_freq from xp_request");

while (repos::fetch(&s1,&sql_id,&req,&freq))
  {
    repos::execute(upd_obj, &req, &sql_id);
    repos::execute(upd_sql, &sql_id, &freq);
  }

repos::close(&s1);

repos::commit();
repos::work_in_progress();

/*******************************************************************************
**  Change the top user to SYSMAN
*******************************************************************************/

repos::execute("update xp_object set xp_name = 'SYSMAN' where xp_parent = 0");

/*******************************************************************************
** Table xp_rep_control - make sure this is the last thing in the file that is
** done
*******************************************************************************/

repos::execute ("update xp_rep_control "
                "set xp_prod_version = :1, xp_repos_version = 414,"
                "xp_rules_loaded = 0",
                current_version);

repos::commit();

repos::work_in_progress();


