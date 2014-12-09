/* Copyright (c) Oracle Corporation, 1996.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       xpoup150.xrl
*
*   DESCRIPTION
*
*       Upgrade script for Oracle Expert
*
*   NOTES
*
*       Upgrades v1.4 repository to V1.5
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_int_t s1, id, tsid, dbid, count;
xp_rowid_t rowid;
xp_str_t str;

/*******************************************************************************
**  xp_symptom_sql
*******************************************************************************/

repos::execute ("create table xp_symptom_sql"
                "("
                "xp_id                   number,"
                "xp_sql_id               number,"
                "xp_user                 varchar(80),"
                "xp_freq                 number,"
                "xp_position             number"
                ")");

/*******************************************************************************
**  xp_sql
*******************************************************************************/

repos::execute ("alter table xp_sql add "
                "xp_owner varchar(80)");

repos::execute ("update xp_sql set xp_owner = ''");

repos::commit();

/*******************************************************************************
**  xp_segment
*******************************************************************************/

repos::execute ("create table xp_segment"
                "("
                "xp_database                   number,"
                "xp_tablespace                 number,"
                "xp_segment                    number,"
                "xp_num_01                     number,"
                "xp_num_02                     number,"
                "xp_num_03                     number,"
                "xp_num_04                     number,"
                "xp_num_05                     number,"
                "xp_num_06                     number,"
                "xp_num_07                     number,"
                "xp_num_08                     number,"
                "xp_num_09                     number,"
                "xp_num_10                     number,"
                "xp_str_01                     varchar(60),"
                "xp_str_02                     varchar(60),"
                "xp_str_03                     varchar(60),"
                "xp_str_04                     varchar(60)"
                ")");

/*******************************************************************************
** For each segment, insert a row into the xp_segment table
*******************************************************************************/

s1 = repos::open ("select xp_id,xp_parent,xp_num_attr_01 "
                  "from xp_object where xp_type=501");

while (success == repos::fetch (&s1, &id, &tsid, &dbid))
  {
    repos::execute 
       ("insert into xp_segment "
           "(xp_database,xp_tablespace,xp_segment,"
              "xp_num_01,xp_num_02,xp_num_03,xp_num_04,xp_num_05,"
              "xp_num_06,xp_num_07,xp_num_08,xp_num_09,xp_num_10,"
              "xp_str_01,xp_str_02,xp_str_03,xp_str_04) "
            "values "
              "(:1,:2,:3,"
              ":4,:4,:4,:4,:4,"
              "0,0,0,0,0,"
              ":5,'','','')",
               &dbid, &tsid, &id,&missing,&missing_str);
 }

repos::close (&s1);

repos::commit();


/*******************************************************************************
** For each table, set new attributes
*******************************************************************************/

repos::execute("alter table xp_table add (xp_str_02 varchar2(60))");

repos::execute("update xp_table set xp_str_01=:1, xp_str_02 = ''",
               &missing_str);

repos::commit();


/*******************************************************************************
** For each index, set xp_str_03 and xp_num_11 to missing
*******************************************************************************/

repos::execute("alter table xp_index add ("
               "xp_num_16 number,"
               "xp_num_17 number,"
               "xp_num_18 number,"
               "xp_num_19 number,"
               "xp_num_20 number)");

repos::execute("update xp_index set "
			   "xp_num_11=:1,xp_str_03=:2,"
               "xp_num_16=0,xp_num_17=0,xp_num_18=0,xp_num_19=0,"
               "xp_num_20=0",
               &missing, &missing_str);

repos::commit();


/*******************************************************************************
** For each cluster, set xp_str_01 to missing
*******************************************************************************/

repos::execute("update xp_cluster set xp_str_01=:1",
               &missing_str);

repos::commit();

/*******************************************************************************
** For each database user, set xp_num_attr_15 to 0 as it is no longer being
** used
*******************************************************************************/

repos::execute("update xp_object set xp_num_attr_15=0"
               "where xp_type=506");

repos::commit();

/*******************************************************************************
** For each tablespace, set xp_num_attr_02 to 0 as it is no longer being
** used (used to be Storage())
*******************************************************************************/

repos::execute("update xp_object set xp_num_attr_02=0"
               "where xp_type=500");

repos::commit();

/*******************************************************************************
** For each segment, set xp_num_attr_05 to 0 as it is no longer being
** used (used to be Storage())
*******************************************************************************/

repos::execute("update xp_object set xp_num_attr_05=0"
               "where xp_type=501");

repos::commit();

/*******************************************************************************
** For each instance, set xp_num_attr_09 to missing, to initialize
*******************************************************************************/

repos::execute("update xp_object set xp_num_attr_09=-999"
               "where xp_type=200");

repos::commit();

/*******************************************************************************
** Encrypt the passwords that were previously stored as literals 
*******************************************************************************/

repos::xp_encrypt_pwd();

repos::commit();

/*******************************************************************************
** For each segment and tablespace, set convert initial and next extent values
** to kbytes.
*******************************************************************************/

repos::execute ("update xp_object set xp_num_attr_03 = ceil(xp_num_attr_03/1024), "
                "xp_num_attr_04 = ceil(xp_num_attr_04/1024) "
                "where xp_type = 500"); 

repos::commit();

repos::execute ("update xp_object set xp_num_attr_06 = ceil(xp_num_attr_06/1024), "
                "xp_num_attr_07 = ceil(xp_num_attr_07/1024) "
                "where xp_type = 501"); 

repos::commit();

/*******************************************************************************
** For every tuning session, update its instance and workload collection
** objects to maintain the count of instances
*******************************************************************************/

xp_int_t incollid,wkcollid;

s1 = repos::open ("select xp_id,xp_num_attr_09,xp_num_attr_12 "
                  "from xp_object where xp_type=10");

while (success == repos::fetch (&s1, &tsid, &incollid, &wkcollid))
  {
    /***************************************************************************
    ** Get the database id for this tuning session
    ***************************************************************************/

    repos::select("select xp_id from xp_object "
                  "where xp_parent=:1 and xp_type=100",1,1,&tsid,&dbid);

    /***************************************************************************
    ** Count the number of instances
    ***************************************************************************/

    count = repos::integer("select count(*) from xp_object "
                           "where xp_parent=:1 and xp_type=200",&dbid);
  
    /***************************************************************************
    ** Update the database's instance and workload collection objects
    ***************************************************************************/

    repos::execute 
       ("update xp_object set xp_num_attr_08=:2 where xp_id=:1", 
        &incollid, &count);

    repos::execute 
       ("update xp_object set xp_num_attr_05=:2 where xp_id=:1", 
        &wkcollid, &count);
 }

repos::close (&s1);

repos::commit();

/*******************************************************************************
** Move instance's xp_str_attr_05 to xp_str_attr_07 for additional length
*******************************************************************************/

s1 = repos::open ("select rowid,xp_str_attr_05 from xp_object "
                  "where xp_type in (200,171,172)");

while (success == repos::fetch (&s1, &rowid, &str))
  {
    repos::execute 
       ("update xp_object set xp_str_attr_07=:2,xp_str_attr_05='' "
        "where rowid=:1", 
        &rowid, &str);
  }

repos::close (&s1);

repos::commit();

/*******************************************************************************
**  xp_transaction table
*******************************************************************************/

repos::execute ("create table xp_transaction"
  "("
    "xp_prog  number,"
    "xp_hash  number,"
    "xp_trans number"
  ")");

s1 = repos::open ("select xp_id, xp_parent from xp_object where xp_type = 703");

while (repos::fetch (&s1, &id, &dbid))
  {
    repos::execute ("insert into xp_transaction (xp_prog,xp_hash,xp_trans) "
                    "values (:1,0,:2)", &dbid, &id);
  }
  
repos::close(&s1);

repos::commit();

/*******************************************************************************
** Table xp_rep_control - make sure this is the last thing in the file that is
** done
*******************************************************************************/

repos::execute ("update xp_rep_control "
                "set xp_prod_version = :1, xp_repos_version = 412,"
                "xp_rules_loaded = 0",
                current_version);

repos::commit();

repos::work_in_progress();

