/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       xpoup160.xrl
*
*   DESCRIPTION
*
*       Upgrade script for Oracle Expert
*
*   NOTES
*
*       Upgrades v1.5 repository to V1.6
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t user, pwd, svc;
xp_int_t id;

/***************************************************************************
** If any Expert database services exist, insert a new invisible
** user object record at top of the Expert hierarchy (if it doesn't
** already exist), and set the parent of each service object to this
** new object.
***************************************************************************/

if (repos::integer ("select count(*) from xp_object "
                    "where xp_parent = 0 and xp_type in (171,172)"))
  {
    repos::user_info (user, pwd, svc);

    id = repos::integer ("select xp_id from xp_object "
                         "where xp_parent = 0 and xp_type = 160 "
                         "and xp_name = UPPER(:1)", &user);
    if (!id)
      {
        id = repos::integer ("select xp_rep_control_sequence.nextval "
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
              "(:1,0,160,UPPER(:2),sysdate,sysdate,0,"
              "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"
              "0,0,0,sysdate,'','','','','','','')",
              &id, &user);
      }

    repos::execute("update xp_object set xp_parent = :1 "
                   "where xp_parent = 0 and xp_type in (171,172)",
                   &id);

    repos::commit();
  }

/*******************************************************************************
**  Create new tmp tables
*******************************************************************************/

repos::execute("create table xp_tmp_cache"
  "("
    "xp_tune                 number,"
    "hash_value              number,"
    "address                 raw(4),"
    "command_type            number,"
    "piece                   number,"
    "sql_text                varchar2(1000)"
  ")");

repos::execute("create table xp_tmp_sql"
  "("
    "xp_tune                 number,"
    "hash_value              number,"
    "address                 raw(4),"
    "disk_reads              number,"
    "executions              number"
  ")");

repos::work_in_progress();

/*******************************************************************************
**  Add ancestry to xp_object
*******************************************************************************/

repos::execute("create table xp_ancestry (xp_id number(15), "
               "xp_ancestry varchar(128))");

xp_int_t parent, s1, count;
xp_char_t buf;
xp_char_t exec = "select xp_parent from xp_object "
                 "where xp_id = :1";
xp_char_t insert = "insert into xp_ancestry (xp_id, xp_ancestry) "
                   "values (:1, :2)";

s1 = repos::open("select xp_parent,xp_id from xp_object");

while (repos::fetch(&s1, &parent, &id))
  {
    buf = concat(id,",",parent);

    while (parent > 0)
      {
        parent = repos::integer(exec,&parent);
        
        buf = concat(buf,",",parent);
      }

    repos::execute(insert, &id, buf);

    count++;

    if ((count % 100) == 0)
      repos::commit();
  }

repos::close(&s1);
repos::commit();

/*******************************************************************************
**  Create new user rule description column
*******************************************************************************/

repos::execute("alter table xp_user_rule add "
               "(xp_desc_id number,xp_help_id number)");
repos::execute("update xp_rep_control set xp_rules_loaded = 0");
repos::execute("drop table xp_user_rule_desc");

/*******************************************************************************
**  XP_SQL_TABLE
*******************************************************************************/

repos::execute("alter table xp_sql_table add "
               "(xp_tune number, xp_mean number, "
               "xp_stddev number, xp_imp number)");

/*******************************************************************************
** For each database, set the bit-map and partition options to missing
*******************************************************************************/

repos::execute("update xp_object set xp_str_attr_06='-999',"
                "xp_str_attr_07='-999'"
               "where xp_type=100");

repos::commit();

repos::work_in_progress();

/*******************************************************************************
** Table xp_rep_control - make sure this is the last thing in the file that is
** done
*******************************************************************************/

repos::execute ("update xp_rep_control "
                "set xp_prod_version = :1, xp_repos_version = 413,"
                "xp_rules_loaded = 0",
                current_version);

repos::commit();

repos::work_in_progress();


