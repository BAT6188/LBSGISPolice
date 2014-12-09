/* Copyright (c) Oracle Corporation, 1998.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       vmqup160.xrl
*
*   DESCRIPTION
*
*       Upgrade script for SQL Analyze
*
*   NOTES
*
*       Upgrades v1.5.5 repository to v1.6.0
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_int_t sel1, sessid, dbid;

/***************************************************************************
** The majority of the table names and column names have changed,
** so upgrade will create new tables using the "AS SELECT" syntax
** to move existing data to the new tables and columns.
**
** The VMQ_SQL_SESSION table has gone away, so all references to its
** session id will be replaced with references to its parent database id.
**
** The execution column found in the various stats tables has been moved into
** its corresponding sql plan parent table.
***************************************************************************/

repos::execute("CREATE TABLE VMQ_DATABASE_ITEM "
               "("
               "database_id PRIMARY KEY,"
               "service"
               ") "
               "AS SELECT "
               "databaseID,"
               "service "
               "FROM VMQ_SQL_DATABASE");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_DATABASE_PARAMS to new VMQ_DATABASE_PARAMS_STATIC
***************************************************************************/

repos::execute("CREATE TABLE VMQ_DATABASE_PARAMS_STATIC "
               "("
               "database_id,"
               "name,"
               "value"
               ") "
               "AS SELECT "
               "databaseID,"
               "name,"
               "value "
               "FROM VMQ_SQL_DATABASE_PARAMS");

repos::execute("ALTER TABLE VMQ_DATABASE_PARAMS_STATIC ADD ("
               "FOREIGN KEY (database_id) "
               "REFERENCES VMQ_DATABASE_ITEM (database_id) ON DELETE CASCADE)");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_SESSION_PARAMS to new VMQ_DATABASE_PARAMS_DYNAMIC
***************************************************************************/

repos::execute("CREATE TABLE VMQ_DATABASE_PARAMS_DYNAMIC "
               "("
               "database_id,"
               "name,"
               "value"
               ") "
               "AS SELECT "
               "ss.databaseID,"
               "ssp.name,"
               "ssp.value "
               "FROM VMQ_SQL_SESSION ss, VMQ_SQL_SESSION_PARAMS ssp "
               "WHERE ss.sessionID = ssp.sessionID AND ssp.sessionid IN ( "
	              "select min(s2.sessionid) from VMQ_SQL_SESSION s2,VMQ_SQL_database s1 "
                  "WHERE s1.databaseid = s2.databaseid(+) group by s2.databaseid "
	           ") "
	       );

repos::execute("ALTER TABLE VMQ_DATABASE_PARAMS_DYNAMIC ADD ("
               "FOREIGN KEY (database_id) "
               "REFERENCES VMQ_DATABASE_ITEM (database_id) ON DELETE CASCADE)");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_OBJECT to new VMQ_SQL_ITEM
***************************************************************************/

repos::execute("CREATE TABLE VMQ_SQL_ITEM "
               "("
               "sqlid PRIMARY KEY,"
               "database_id,"
               "label,"
               "group_name,"
               "optimizer_choice"
               ") "
               "AS SELECT "
               "so.sqlID,"
               "ss.databaseID,"
               "so.label,"
               "so.groupName,"
               "so.optimizer_choice "
               "FROM VMQ_SQL_SESSION ss, VMQ_SQL_OBJECT so "
               "WHERE ss.sessionID = so.sessionID");

repos::execute("ALTER TABLE VMQ_SQL_ITEM ADD (schema_context VARCHAR2(255), "
               "FOREIGN KEY (database_id) "
               "REFERENCES VMQ_DATABASE_ITEM (database_id) ON DELETE CASCADE)");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_TEXT
***************************************************************************/

repos::execute("CREATE TABLE VMQ_TEMP "
               "("
               "sqlid,"
               "piece,"
               "sql_text"
               ") "
               "AS SELECT "
               "sqlID,"
               "piece,"
               "sqlText "
               "FROM VMQ_SQL_TEXT");

repos::execute("ALTER TABLE VMQ_TEMP ADD ("
               "FOREIGN KEY (sqlid) "
               "REFERENCES VMQ_SQL_ITEM (sqlid) ON DELETE CASCADE)");

repos::execute("DROP TABLE VMQ_SQL_TEXT CASCADE CONSTRAINTS");

repos::execute("RENAME VMQ_TEMP TO VMQ_SQL_TEXT");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_IMPORT_STATS
***************************************************************************/

repos::execute("CREATE TABLE VMQ_TEMP "
               "("
               "sqlid,"
               "name,"
               "value"
               ") "
               "AS SELECT "
               "sqlID,"
               "name,"
               "value "
               "FROM VMQ_SQL_IMPORT_STATS");

repos::execute("ALTER TABLE VMQ_TEMP ADD ("
               "FOREIGN KEY (sqlid) "
               "REFERENCES VMQ_SQL_ITEM (sqlid) ON DELETE CASCADE)");

repos::execute("DROP TABLE VMQ_SQL_IMPORT_STATS CASCADE CONSTRAINTS");

repos::execute("RENAME VMQ_TEMP TO VMQ_SQL_IMPORT_STATS");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_PLAN_RULE
***************************************************************************/

repos::execute("ALTER TABLE VMQ_SQL_PLAN_RULE ADD (executions NUMBER, "
               "FOREIGN KEY (sqlid) "
               "REFERENCES VMQ_SQL_ITEM (sqlid) ON DELETE CASCADE)");

repos::execute("UPDATE VMQ_SQL_PLAN_RULE sp SET sp.executions = "
               "(SELECT DISTINCT executionTimes FROM VMQ_SQL_STATS_RULE ss "
               "WHERE ss.sqlid = sp.sqlid)");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_PLAN_COST_FIRST
***************************************************************************/

repos::execute("ALTER TABLE VMQ_SQL_PLAN_COST_FIRST ADD (executions NUMBER, "
               "FOREIGN KEY (sqlid) "
               "REFERENCES VMQ_SQL_ITEM (sqlid) ON DELETE CASCADE)");

repos::execute("UPDATE VMQ_SQL_PLAN_COST_FIRST sp SET sp.executions = "
               "(SELECT DISTINCT executionTimes FROM VMQ_SQL_STATS_COST_FIRST ss "
               "WHERE ss.sqlid = sp.sqlid)");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_PLAN_COST_ALL
***************************************************************************/

repos::execute("ALTER TABLE VMQ_SQL_PLAN_COST_ALL ADD (executions NUMBER, "
               "FOREIGN KEY (sqlid) "
               "REFERENCES VMQ_SQL_ITEM (sqlid) ON DELETE CASCADE)");

repos::execute("UPDATE VMQ_SQL_PLAN_COST_ALL sp SET sp.executions = "
               "(SELECT DISTINCT executionTimes FROM VMQ_SQL_STATS_COST_ALL ss "
               "WHERE ss.sqlid = sp.sqlid)");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_STATS_RULE
***************************************************************************/

repos::execute("CREATE TABLE VMQ_TEMP "
               "("
               "sqlid,"
               "statistic_type,"
               "elapsed_time,"
               "cpu_time,"
               "logical_blk_rds,"
               "physical_blk_rds,"
               "recursive_calls,"
               "database_calls,"
               "chained_rows,"
               "rows_returned,"
               "uga_memory,"
               "pga_memory,"
               "db_blk_changes,"
               "redo_entries,"
               "redo_size,"
               "rows_gotten,"
               "sorts_memory,"
               "sorts_disk,"
               "sorts_rows"
               ") "
               "AS SELECT "
               "sqlID,"
               "type,"
               "elapsedTime,"
               "cpuTime,"
               "logicalBlkRds,"
               "physicalBlkRds,"
               "recursiveCalls,"
               "databaseCalls,"
               "chainedRows,"
               "rowsReturned,"
               "ugaMemory,"
               "pagMemory,"
               "databaseBlks,"
               "redoEntries,"
               "redoSize,"
               "rowsGotten,"
               "sortsMemory,"
               "sortsDisk,"
               "sortsRow "
               "FROM VMQ_SQL_STATS_RULE");

repos::execute("ALTER TABLE VMQ_TEMP ADD ("
               "FOREIGN KEY (sqlid) "
               "REFERENCES VMQ_SQL_ITEM (sqlid) ON DELETE CASCADE)");

repos::execute("DROP TABLE VMQ_SQL_STATS_RULE CASCADE CONSTRAINTS");

repos::execute("RENAME VMQ_TEMP TO VMQ_SQL_STATS_RULE");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_STATS_COST_FIRST
***************************************************************************/

repos::execute("CREATE TABLE VMQ_TEMP "
               "("
               "sqlid,"
               "statistic_type,"
               "elapsed_time,"
               "cpu_time,"
               "logical_blk_rds,"
               "physical_blk_rds,"
               "recursive_calls,"
               "database_calls,"
               "chained_rows,"
               "rows_returned,"
               "uga_memory,"
               "pga_memory,"
               "db_blk_changes,"
               "redo_entries,"
               "redo_size,"
               "rows_gotten,"
               "sorts_memory,"
               "sorts_disk,"
               "sorts_rows"
               ") "
               "AS SELECT "
               "sqlID,"
               "type,"
               "elapsedTime,"
               "cpuTime,"
               "logicalBlkRds,"
               "physicalBlkRds,"
               "recursiveCalls,"
               "databaseCalls,"
               "chainedRows,"
               "rowsReturned,"
               "ugaMemory,"
               "pagMemory,"
               "databaseBlks,"
               "redoEntries,"
               "redoSize,"
               "rowsGotten,"
               "sortsMemory,"
               "sortsDisk,"
               "sortsRow "
               "FROM VMQ_SQL_STATS_COST_FIRST");

repos::execute("ALTER TABLE VMQ_TEMP ADD ("
               "FOREIGN KEY (sqlid) "
               "REFERENCES VMQ_SQL_ITEM (sqlid) ON DELETE CASCADE)");

repos::execute("DROP TABLE VMQ_SQL_STATS_COST_FIRST CASCADE CONSTRAINTS");

repos::execute("RENAME VMQ_TEMP TO VMQ_SQL_STATS_COST_FIRST");

repos::work_in_progress();


/***************************************************************************
** Upgrade VMQ_SQL_STATS_COST_ALL
***************************************************************************/

repos::execute("CREATE TABLE VMQ_TEMP "
               "("
               "sqlid,"
               "statistic_type,"
               "elapsed_time,"
               "cpu_time,"
               "logical_blk_rds,"
               "physical_blk_rds,"
               "recursive_calls,"
               "database_calls,"
               "chained_rows,"
               "rows_returned,"
               "uga_memory,"
               "pga_memory,"
               "db_blk_changes,"
               "redo_entries,"
               "redo_size,"
               "rows_gotten,"
               "sorts_memory,"
               "sorts_disk,"
               "sorts_rows"
               ") "
               "AS SELECT "
               "sqlID,"
               "type,"
               "elapsedTime,"
               "cpuTime,"
               "logicalBlkRds,"
               "physicalBlkRds,"
               "recursiveCalls,"
               "databaseCalls,"
               "chainedRows,"
               "rowsReturned,"
               "ugaMemory,"
               "pagMemory,"
               "databaseBlks,"
               "redoEntries,"
               "redoSize,"
               "rowsGotten,"
               "sortsMemory,"
               "sortsDisk,"
               "sortsRow "
               "FROM VMQ_SQL_STATS_COST_ALL");

repos::execute("ALTER TABLE VMQ_TEMP ADD ("
               "FOREIGN KEY (sqlid) "
               "REFERENCES VMQ_SQL_ITEM (sqlid) ON DELETE CASCADE)");

repos::execute("DROP TABLE VMQ_SQL_STATS_COST_ALL CASCADE CONSTRAINTS");

repos::execute("RENAME VMQ_TEMP TO VMQ_SQL_STATS_COST_ALL");

repos::work_in_progress();


/***************************************************************************
** Create a new table VMQ_SQL_UNQUALIFIED_NAMES. This table did not exist
** in version 1.5.5.
***************************************************************************/

repos::execute("CREATE TABLE VMQ_SQL_UNQUALIFIED_NAMES "
	       "("
	       "sqlid		NUMBER, "
	       "table_name	VARCHAR2(255), "
	       "FOREIGN KEY (sqlid) "
	       "REFERENCES VMQ_SQL_ITEM(sqlid) ON DELETE CASCADE"
	       ")");

repos::work_in_progress();


/***************************************************************************
** Drop old tables
***************************************************************************/
           
repos::execute("DROP TABLE VMQ_SQL_OBJECT CASCADE CONSTRAINTS");
repos::execute("DROP TABLE VMQ_SQL_SESSION_PARAMS CASCADE CONSTRAINTS");
repos::execute("DROP TABLE VMQ_SQL_DATABASE_PARAMS CASCADE CONSTRAINTS");
repos::execute("DROP TABLE VMQ_SQL_SESSION CASCADE CONSTRAINTS");
repos::execute("DROP TABLE VMQ_SQL_DATABASE CASCADE CONSTRAINTS");

repos::work_in_progress();


/***************************************************************************
** Create new indexes
***************************************************************************/

repos::execute("CREATE INDEX i_vmq_sql_plan_rl "
               "ON vmq_sql_plan_rule(sqlid ASC)");

repos::execute("CREATE INDEX i_vmq_sql_plan_cf "
               "ON vmq_sql_plan_cost_first(sqlid ASC)");

repos::execute("CREATE INDEX i_vmq_sql_plan_ca "
               "ON vmq_sql_plan_cost_all(sqlid ASC)");

repos::execute("CREATE INDEX i_vmq_sql_stats_rl "
               "ON vmq_sql_stats_rule(sqlid ASC, statistic_type ASC)");

repos::execute("CREATE INDEX i_vmq_sql_stats_cf "
               "ON vmq_sql_stats_cost_first(sqlid ASC, statistic_type ASC)");

repos::execute("CREATE INDEX i_vmq_sql_stats_ca "
               "ON vmq_sql_stats_cost_all(sqlid ASC, statistic_type ASC)");

repos::work_in_progress();


/***************************************************************************
** Commit just in case something gets added that isn't auto commit.
***************************************************************************/

repos::commit();
