/* Copyright (c) Oracle Corporation, 1998.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vmqmigr.xrl
*
*   DESCRIPTION
*
*       Migration script for the Sql Analyze 2.1.0 repository vmq 
*       tables.  
*
*   NOTES
*
*
*******************************************************************************/

printf(get_text(xprc_gl_wip_mig_vmq_head));

///////////////////////////////////////
// Local Variables
///////////////////////////////////////

xp_int_t v2id;

xp_int_t cursor1,
         cursor2,
         cursor3;

xp_int_t ndbid,
         nunique_sql_id,
         nsqlid,
         noptimizer_choice,
         i,
         npiece;

xp_str_t sservice,
         sname,
         svalue,
         sschema_context,
         slabel,
         sgroup_name,
         stext;

xp_str_t sstatement_id,
         sremarks,
         soperation,
         soptions,
         sobject_node,
         sobject_owner,
         sobject_name,
         sobject_type,
         soptimizer,
         scardinality,
         sother_tag,
         spartition_start,
         spartition_stop;

xp_int_t nobject_instance,
         nsearch_columns,
         nid,
         nparent_id,
         nposition,
         ncost,
         nbytes,
         npartition_id,
         nexecutions;

xp_int_t nstats_type,
         nlogical_blk_read,
         nphy_blk_read,
         nrecursive_calls,
         ndb_calls,
         nchained_rows, 
         nrows_returned,
         nuga_memory,
         npga_memory,
         ndb_blk_changes, 
         nredo_entries,
         nredo_size,
         nrows_gotten,
         nsorts_memory, 
         nsorts_disk,
         nsorts_rows;

xp_str_t sother,
         userancestry,
         serviceancestry;

xp_date_t dtimestamp;

xp_float_t felapsed_time,
           fcpu_time;

///////////////////////////////////////////////////


////////////////////////////////
// If the OMS user already exists, get the oms user id
// from vdk_object otherwise generate a new one
////////////////////////////////
xp_int_t count;
count = target::integer("SELECT COUNT(*) "
                        "FROM vdk_object "
                        "WHERE vdk_parent_id = 0 "
                        "AND vdk_type = 160 "
                        "AND vdk_name = UPPER(:1)", v2user);
 
if (count == 0)
  {

    v2id = target::integer("SELECT vdk_rep_control_sequence.nextval "
                           "FROM vdk_rep_control");

    ////////////////////////////////
    // generate the ancestry id list for the OMS user (parent is always 0)
    ////////////////////////////////
    userancestry = concat(v2id, ",0");

    ////////////////////////////////
    // enter the OMS user into the vdk_object table
    ////////////////////////////////
    target::execute
        ("INSERT INTO vdk_object "
         "(vdk_id, vdk_parent_id, vdk_type, vdk_name, vdk_ancestry_ids,"
         " vdk_cre_date, vdk_mod_date, vdk_creation_status, vdk_working_status,"
         " vdk_is_hidden, vdk_is_incomplete, vdk_is_invalid, vdk_is_collect,"
         " vdk_udr_exists) "
         "VALUES "
         "(:1, 0, 160, UPPER(:2), :3, SYSDATE, SYSDATE, 0,"
         " 0, 0, 0, 0, 0, 0)",
         v2id, v2user, userancestry);
  }

v2id = target::integer("SELECT vdk_id "
                       "FROM vdk_object "
                       "WHERE vdk_parent_id = 0 "
                       "AND vdk_type = 160 "
                       "AND vdk_name = UPPER(:1)",
                       v2user);
////////////////////////////////
// generate the ancestry id list for the OMS user (parent is always 0)
////////////////////////////////
userancestry = concat(v2id, ",0");

////////////////////////////////
// Copy the services from vmq_database_item to xp_object
////////////////////////////////
xp_int_t nNewDbId;

cursor1 = repos::open("SELECT * "
                      "FROM vmq_database_item "
                      "ORDER BY database_id");

while (repos::fetch(&cursor1,
                    &ndbid, sservice))
  {
    ////////////////////////////////
    // select any existing service (based on service name equality) from the
    // migrated repository
    ////////////////////////////////
    cursor2 = target::open("SELECT vdk_id "
                           "FROM vdk_object "
                           "WHERE vdk_parent_id = :1 "
                           "AND vdk_name = :2 "
                           "AND vdk_type = 171",
                           v2id, sservice);

    ////////////////////////////////
    // if the service did not exist...
    ////////////////////////////////
    if (!target::fetch(&cursor2,
                       &nNewDbId))
    {
      ////////////////////////////////
      // ...get a new id for the service object to insert,...
      ////////////////////////////////
      nNewDbId = target::integer("SELECT vdk_rep_control_sequence.nextval "
                                 "FROM vdk_rep_control");

      ////////////////////////////////
      // ...generate the ancestry id list,...
      ////////////////////////////////
      serviceancestry = concat(nNewDbId, ",", userancestry);

      ////////////////////////////////
      // ...insert the object into the vdk_object table and...
      ////////////////////////////////
      target::execute
          ("INSERT INTO vdk_object "
           "(vdk_id, vdk_parent_id, vdk_type, vdk_name, vdk_ancestry_ids,"
           " vdk_cre_date, vdk_mod_date, vdk_creation_status, vdk_working_status,"
           " vdk_is_hidden, vdk_is_incomplete, vdk_is_invalid, vdk_is_collect,"
           " vdk_udr_exists) "
           "VALUES "
           "(:1, :2, 171, :3, :4, SYSDATE, SYSDATE, 0,"
           " 0, 0, 0, 0, 0, 0)",
           nNewDbId, v2id, sservice, serviceancestry);

      ////////////////////////////////
      // ...insert other service information into the vdk_service table
      ////////////////////////////////
      target::execute
          ("INSERT INTO vdk_service "
           "(vdk_id, vdk_flags, vdk_attr_01, vdk_parent_id, vdk_create,"
           " vdk_attr_02, vdk_attr_03, vdk_attr_04, vdk_attr_05, vdk_attr_06) "
           "VALUES "
           "(:1, 1, :2, :3, 2, '', '', '', '', '')",
           nNewDbId, sservice, v2id);
    }

    ////////////////////////////////
    // (if the service did exist, nNewDbId represents the id of the existing
    //  service; in either case nNewDbId reflects the migrated value)
    ////////////////////////////////

    ////////////////////////////////
    // Migrate SQL objects from VMQ_SQL_ITEM
    ////////////////////////////////
    cursor2 = repos::open("SELECT sqlid, label, group_name, optimizer_choice, schema_context "
                          "FROM vmq_sql_item "
                          "WHERE database_id = :1 "
                          "ORDER BY sqlid",
                          ndbid); 

    ////////////////////////////////
    // For each sql under this database
    ////////////////////////////////
    nunique_sql_id = target::integer("SELECT COUNT(*) "
                        "FROM vmq_sql_item ");
                        
    while (repos::fetch(&cursor2,
                        &nsqlid, slabel, sgroup_name, &noptimizer_choice,
                        sschema_context))
      {
        ////////////////////////////////
        // update VMQ_SQL_ITEM in target repository
        ////////////////////////////////
        target::execute
            ("INSERT INTO vmq_sql_item "
             "VALUES (:1, :2, :3, :4, :5, :6, -1)",
             nNewDbId, nunique_sql_id, slabel, sgroup_name, noptimizer_choice,
             sschema_context);

        ////////////////////////////////
        // Update the VMQ_SQL_TEXT
        ////////////////////////////////
        cursor3 = repos::open("SELECT * "
                              "FROM vmq_sql_text "
                              "WHERE sqlid = :1 "
                              "ORDER BY sqlid, piece",
                              nsqlid);

        while (repos::fetch(&cursor3,
                            &i, &npiece, stext))
          {
            target::execute
                ("INSERT INTO vmq_sql_text "
                 "VALUES (:1, :2, :3, :4)",
                 nNewDbId, nunique_sql_id, npiece, stext);  
          }
        repos::close(&cursor3);

        ////////////////////////////////
        // Update VMQ_SQL_UNQUALIFIED_NAMES
        ////////////////////////////////
        cursor3 = repos::open("SELECT * "
                              "FROM vmq_sql_unqualified_names "
                              "WHERE sqlid = :1",
                              nsqlid);
        while (repos::fetch(&cursor3,
                            &i, sname))
          {
            target::execute
                ("INSERT INTO VMQ_SQL_UNQUALIFIED_NAMES "
                 "VALUES (:1, :2, :3)",
                 nNewDbId, nunique_sql_id, sname);
          }
        repos::close(&cursor3);

        ////////////////////////////////
        // Update VMQ_SQL_IMPORT_STATS
        ////////////////////////////////
        cursor3 = repos::open("SELECT * "
                              "FROM vmq_sql_import_stats "
                              "WHERE sqlid = :1",
                              nsqlid);
        while (repos::fetch(&cursor3,
                            &i, sname, svalue))
          {
            target::execute
                ("INSERT INTO vmq_sql_import_stats "
                 "VALUES (:1, :2, :3, :4)",
                 nNewDbId, nunique_sql_id, sname, svalue);
          }
        repos::close(&cursor3);

        ////////////////////////////////
        // Update VMQ_SQL_PLAN_RULE
        ////////////////////////////////
   	    cursor3 = repos::open("SELECT sqlid, statement_id, remarks,"  
                              " operation, options, object_node, object_owner, object_name,"
                              " object_instance, object_type, optimizer, search_columns, id,"
                              " parent_id, position, cost, cardinality, bytes, other_tag,"
                              " partition_start ,partition_stop, partition_id, other, executions " 
                              "FROM vmq_sql_plan_rule "
                              "WHERE sqlid = :1 "
                              "ORDER BY id",
                              nsqlid);

        while (repos::fetch(&cursor3,
                            &i, sstatement_id, sremarks, soperation, soptions,
                            sobject_node, sobject_owner, sobject_name, &nobject_instance,
                            sobject_type, soptimizer, &nsearch_columns, &nid, &nparent_id, &nposition,
                            &ncost, scardinality, &nbytes, sother_tag, spartition_start, spartition_stop,
                            &npartition_id, sother, &nexecutions))
          {
            target::execute
                ("INSERT INTO vmq_sql_plan_rule "
                 "VALUES "
	               "(:1, :2, NULL, NULL, NULL, :3, :4, :5, :6, :7, NULL, :8,"
                 " NULL, NULL, :9, :10, :11, NULL, :12, NULL, :13, :14, :15,"
                 " :16, :17, :18, NULL)",
                 nNewDbId, nunique_sql_id, soperation, soptions,
                 sobject_node, sobject_owner, sobject_name, sobject_type,
                 nid, nparent_id, nposition, scardinality, sother_tag,
                 spartition_start, spartition_stop, npartition_id, sother, nexecutions);
          }
        repos::close(&cursor3);

        ////////////////////////////////
        // Update VMQ_SQL_PLAN_COST_FIRST
        ////////////////////////////////
   	    cursor3 = repos::open("SELECT sqlid, statement_id, remarks,"  
                              " operation, options, object_node, object_owner, object_name,"
                              " object_instance, object_type, optimizer, search_columns, id,"
                              " parent_id, position, cost, cardinality, bytes, other_tag,"
                              " partition_start ,partition_stop, partition_id, other, executions " 
                              "FROM vmq_sql_plan_cost_first "
                              "WHERE sqlid = :1 "
                              "ORDER BY id",
                              nsqlid);

        while (repos::fetch(&cursor3,
                            &i, sstatement_id, sremarks, soperation, soptions,
                            sobject_node, sobject_owner, sobject_name, &nobject_instance,
                            sobject_type, soptimizer, &nsearch_columns, &nid, &nparent_id, &nposition,
                            &ncost, scardinality, &nbytes, sother_tag, spartition_start, spartition_stop,
                            &npartition_id, sother, &nexecutions))
          {
            target::execute
                ("INSERT INTO vmq_sql_plan_cost_first "
                 "VALUES "
	               "(:1, :2, NULL, NULL, NULL, :3, :4, :5, :6, :7, NULL, :8,"
                 " NULL, NULL, :9, :10, :11, NULL, :12, NULL, :13, :14, :15,"
                 " :16, :17, :18, NULL)",
                 nNewDbId, nunique_sql_id, soperation, soptions,
                 sobject_node, sobject_owner, sobject_name, sobject_type,
                 nid, nparent_id, nposition, scardinality, sother_tag,
                 spartition_start, spartition_stop, npartition_id, sother, nexecutions);
          }
        repos::close(&cursor3);

        ////////////////////////////////
        // Update VMQ_SQL_PLAN_COST_ALL
        ////////////////////////////////
   	    cursor3 = repos::open("SELECT sqlid, statement_id, remarks,"  
                              " operation, options, object_node, object_owner, object_name,"
                              " object_instance, object_type, optimizer, search_columns, id,"
                              " parent_id, position, cost, cardinality, bytes, other_tag,"
                              " partition_start ,partition_stop, partition_id, other, executions " 
                              "FROM vmq_sql_plan_cost_all "
                              "WHERE sqlid = :1 "
                              "ORDER BY id",
                              nsqlid);

        while (repos::fetch(&cursor3,
                            &i, sstatement_id, sremarks, soperation, soptions,
                            sobject_node, sobject_owner, sobject_name, &nobject_instance,
                            sobject_type, soptimizer, &nsearch_columns, &nid, &nparent_id, &nposition,
                            &ncost, scardinality, &nbytes, sother_tag, spartition_start, spartition_stop,
                            &npartition_id, sother, &nexecutions))
          {
            target::execute
                ("INSERT INTO vmq_sql_plan_cost_all "
                 "VALUES "
	               "(:1, :2, NULL, NULL, NULL, :3, :4, :5, :6, :7, NULL, :8,"
                 " NULL, NULL, :9, :10, :11, NULL, :12, NULL, :13, :14, :15,"
                 " :16, :17, :18, NULL)",
                 nNewDbId, nunique_sql_id, soperation, soptions,
                 sobject_node, sobject_owner, sobject_name, sobject_type,
                 nid, nparent_id, nposition, scardinality, sother_tag,
                 spartition_start, spartition_stop, npartition_id, sother, nexecutions);
          }
        repos::close(&cursor3);

        ////////////////////////////////
        // Update VMQ_SQL_STATS_RULE
        ////////////////////////////////
        cursor3 = repos::open("SELECT * "
                              "FROM vmq_sql_stats_rule "
                              "WHERE sqlid = :1 "
                              "ORDER BY sqlid, statistic_type",
                              nsqlid);
                            
        while (repos::fetch(&cursor3,
                            &i, &nstats_type, &felapsed_time, &fcpu_time, &nlogical_blk_read,
                            &nphy_blk_read, &nrecursive_calls, &ndb_calls, &nchained_rows, &nrows_returned,
                            &nuga_memory, &npga_memory, &ndb_blk_changes, &nredo_entries, &nredo_size,
                            &nrows_gotten, &nsorts_memory, &nsorts_disk, &nsorts_rows))
          {
            target::execute
                ("INSERT INTO vmq_sql_stats_rule "
                 "VALUES "
	               "(:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13, :14,"
                 " :15, :16, :17, :18, :19, :20)",
                 nNewDbId, nunique_sql_id,
                 nstats_type, felapsed_time, fcpu_time, nlogical_blk_read,
                 nphy_blk_read, nrecursive_calls, ndb_calls, nchained_rows, nrows_returned,
                 nuga_memory, npga_memory, ndb_blk_changes, nredo_entries, nredo_size,
                 nrows_gotten, nsorts_memory, nsorts_disk, nsorts_rows);
          }
        repos::close(&cursor3);

        ////////////////////////////////
        // Update VMQ_SQL_STATS_COST_FIRST
        ////////////////////////////////
        cursor3 = repos::open("SELECT * "
                              "FROM vmq_sql_stats_cost_first "
                              "WHERE sqlid = :1 "
                              "ORDER BY sqlid, statistic_type",
                              nsqlid);
                            
        while (repos::fetch(&cursor3,
                            &i, &nstats_type, &felapsed_time, &fcpu_time, &nlogical_blk_read,
                            &nphy_blk_read, &nrecursive_calls, &ndb_calls, &nchained_rows, &nrows_returned,
                            &nuga_memory, &npga_memory, &ndb_blk_changes, &nredo_entries, &nredo_size,
                            &nrows_gotten, &nsorts_memory, &nsorts_disk, &nsorts_rows))
          {
            target::execute
                ("INSERT INTO vmq_sql_stats_cost_first "
                 "VALUES "
	               "(:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13, :14,"
                 " :15, :16, :17, :18, :19, :20)",
                 nNewDbId, nunique_sql_id, 
                 nstats_type, felapsed_time, fcpu_time, nlogical_blk_read,
                 nphy_blk_read, nrecursive_calls, ndb_calls, nchained_rows, nrows_returned,
                 nuga_memory, npga_memory, ndb_blk_changes, nredo_entries, nredo_size,
                 nrows_gotten, nsorts_memory, nsorts_disk, nsorts_rows);
          }
        repos::close(&cursor3);

        ////////////////////////////////
        // Update VMQ_SQL_STATS_COST_ALL
        ////////////////////////////////
        cursor3 = repos::open("SELECT * "
                              "FROM vmq_sql_stats_cost_all "
                              "WHERE sqlid = :1 "
                              "ORDER BY sqlid, statistic_type",
                              nsqlid);
                            
        while (repos::fetch(&cursor3,
                            &i, &nstats_type, &felapsed_time, &fcpu_time, &nlogical_blk_read,
                            &nphy_blk_read, &nrecursive_calls, &ndb_calls, &nchained_rows, &nrows_returned,
                            &nuga_memory, &npga_memory, &ndb_blk_changes, &nredo_entries, &nredo_size,
                            &nrows_gotten, &nsorts_memory, &nsorts_disk, &nsorts_rows))
          {
            target::execute
                ("INSERT INTO vmq_sql_stats_cost_all "
                 "VALUES "
	               "(:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13, :14,"
                 " :15, :16, :17, :18, :19, :20)",
                 nNewDbId, nunique_sql_id, 
                 nstats_type, felapsed_time, fcpu_time, nlogical_blk_read,
                 nphy_blk_read, nrecursive_calls, ndb_calls, nchained_rows, nrows_returned,
                 nuga_memory, npga_memory, ndb_blk_changes, nredo_entries, nredo_size,
                 nrows_gotten, nsorts_memory, nsorts_disk, nsorts_rows);
          }
        repos::close(&cursor3);

        nunique_sql_id = nunique_sql_id + 1;
      }
    repos::close(&cursor2);
  }
repos::close(&cursor1); 
