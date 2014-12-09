/*  Copyright (c) 1996, 1998, 2000 by Oracle Corporation
/******************************************************************************
**
**  NAME
**         epcup160.xrl
**
**
**  FUNCTION
**
**       Upgrades 1.5.5 the Oracle Trace repository tables to 
**       1.6.0 Trace tables
**
**  NOTES
**
*******************************************************************************/

xp_str_t user, pwd, svc;

xp_int_t user_id = 7;


/******************************************************************************
**  Create sequence for user ids
*******************************************************************************/

repos::execute("CREATE SEQUENCE epc_cli_rep_users_sequence "
                   "START WITH 20 "
                   "INCREMENT BY 1 "
                   "NOMINVALUE "
                   "MAXVALUE 999999999 "
                   "ORDER "
                   "CYCLE "
                   "CACHE 50");

/******************************************************************************
** This tables maintains a list of repository users and his/her unique id.
** Initial name and id are set by getting the repository username and
** arbitrarily assigning the number "1" to the first user.
*******************************************************************************/


repos::user_info (user, pwd, svc);



repos::execute("CREATE TABLE epc_cli_rep_users ("
                    "rep_user_id NUMBER "
                        "CONSTRAINT epc_cli_rep_users_c1 PRIMARY KEY, "
                    "rep_user_name	VARCHAR2(100) "
                        "CONSTRAINT epc_cli_rep_users_c2 UNIQUE)");

repos::execute("INSERT INTO epc_cli_rep_users (rep_user_id, rep_user_name) "
                    "VALUES (:1, LOWER(:2))", user_id, user);


/*******************************************************************************
**  Drop all of the old primary key constraints (they will be readded 
**  with the new rep_user_id added to key(s)).
*******************************************************************************/

repos::execute ("ALTER TABLE epc_cli_job "
                    "DROP CONSTRAINT epc_cli_job_c1 CASCADE");

repos::execute ("ALTER TABLE epc_cli_node "
                    "DROP CONSTRAINT epc_cli_node_c1 CASCADE "
                    "DROP CONSTRAINT epc_cli_node_c2 CASCADE");

repos::execute ("ALTER TABLE epc_cli_environment "
                    "DROP CONSTRAINT epc_cli_environment_c1 CASCADE");

repos::execute ("ALTER TABLE epc_cli_service "
                    "DROP CONSTRAINT epc_cli_service_c1 CASCADE "
                    "DROP CONSTRAINT epc_cli_service_c2 CASCADE");

repos::execute ("ALTER TABLE epc_cli_fdf_file "
                    "DROP CONSTRAINT epc_cli_fdf_file_c1 CASCADE "
                    "DROP CONSTRAINT epc_cli_fdf_file_c4 CASCADE");

repos::execute ("ALTER TABLE epc_cli_collection "
                    "DROP CONSTRAINT epc_cli_collection_c1 CASCADE "
                    "DROP CONSTRAINT epc_cli_collection_c2 CASCADE");

repos::execute ("ALTER TABLE epc_cli_progress "
                    "DROP CONSTRAINT epc_cli_progress_c1 CASCADE");

repos::execute ("ALTER TABLE epc_cli_format "
                    "DROP CONSTRAINT epc_cli_format_c1 CASCADE");





/******************************************************************************
**
**  Redefine constraints to include the new rep_user_id column
**
******************************************************************************/

/* epc_cli_job table
********************/

repos::execute ("ALTER TABLE epc_cli_job "
                    "ADD (rep_user_id NUMBER CONSTRAINT epc_cli_job_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_job SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_job "
                    "ADD (CONSTRAINT  epc_cli_job_c1 "
                    "PRIMARY KEY (rep_user_id, job_id))");



/* epc_cli_node table
*********************/

repos::execute ("ALTER TABLE epc_cli_node "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_node_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_node SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_node "
                    "ADD (CONSTRAINT  epc_cli_node_c1 "
                    "PRIMARY KEY (rep_user_id, node_id))");

repos::execute ("ALTER TABLE epc_cli_node "
                    "ADD (CONSTRAINT  epc_cli_node_c2 "
                    "UNIQUE (rep_user_id, node_name))");

/* epc_cli_environment table
****************************/

repos::execute ("ALTER TABLE epc_cli_environment "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_environment_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_environment SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_environment "
                    "ADD (CONSTRAINT epc_cli_environment_c1 PRIMARY KEY (rep_user_id, environment_id), "
                    "CONSTRAINT epc_cli_environtment_c2 FOREIGN KEY (rep_user_id, node_id) "
                        "REFERENCES epc_cli_node (rep_user_id, node_id) "
                    "ON DELETE CASCADE)");

/* epc_cli_service table
************************/

repos::execute ("ALTER TABLE epc_cli_service "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_service_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_service SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_service "
                    "ADD (CONSTRAINT epc_cli_service_c1 PRIMARY KEY (rep_user_id, service_id), "
                         "CONSTRAINT epc_cli_service_c2 UNIQUE(rep_user_id, service_name, environment_id), "
                         "CONSTRAINT epc_cli_service_c3 FOREIGN KEY (rep_user_id, environment_id) "
                            "REFERENCES epc_cli_environment (rep_user_id, environment_id) "
                            "ON DELETE CASCADE)");
    

/* epc_cli_fdf_file table
*************************/

repos::execute ("ALTER TABLE epc_cli_fdf_file "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_fdf_file_c0 REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_fdf_file SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_fdf_file "
                    "ADD (CONSTRAINT epc_cli_fdf_file_c1 PRIMARY KEY (rep_user_id, fdf_file_id), "
                    "CONSTRAINT epc_cli_fdf_file_c4 UNIQUE (rep_user_id, environment_id,product_name,event_set_name), "
                    "CONSTRAINT epc_cli_fdf_file_c3 FOREIGN KEY (rep_user_id, environment_id) "
                    "REFERENCES epc_cli_environment (rep_user_id, environment_id) "
                    "ON DELETE CASCADE)");


/* epc_cli_collection table
***************************/

repos::execute ("ALTER TABLE epc_cli_collection "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_collection_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");


repos::execute ("UPDATE epc_cli_collection SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_collection "
                    "ADD (CONSTRAINT epc_cli_collection_c1 PRIMARY KEY (rep_user_id, collection_id), "
                    "CONSTRAINT epc_cli_collection_c2 UNIQUE (rep_user_id, collection_name), "
                    "CONSTRAINT epc_cli_collection_c3 FOREIGN KEY (rep_user_id, environment_id) "
                    "REFERENCES epc_cli_environment (rep_user_id, environment_id) "
                    "ON DELETE CASCADE)");

/* epc_cli_progress table
*************************/

repos::execute ("ALTER TABLE epc_cli_progress "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_progress_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_progress SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_progress "
                    "ADD (CONSTRAINT epc_cli_progress_c1 "
                    "PRIMARY KEY (rep_user_id, progress_id), "
                    "CONSTRAINT epc_cli_progress_c2 "
                    "FOREIGN KEY (rep_user_id, collection_id) "
                    "REFERENCES epc_cli_collection(rep_user_id, collection_id) "
                    "ON DELETE CASCADE)");

/* epc_cli_format table
***********************/

repos::execute ("ALTER TABLE epc_cli_format "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_format_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_format SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_format "
                    "ADD (CONSTRAINT epc_cli_format_c1 "
                    "PRIMARY KEY (rep_user_id, format_id), "
                    "CONSTRAINT epc_cli_format_c2 "
                    "FOREIGN KEY (rep_user_id, collection_id) "
                    "REFERENCES epc_cli_collection(rep_user_id, collection_id) "
                    "ON DELETE CASCADE)");


/* epc_cli_usage table
**********************/

repos::execute ("ALTER TABLE epc_cli_usage "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_usage_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_usage SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_usage "
                    "ADD (CONSTRAINT epc_cli_usage_c1 "
                    "FOREIGN KEY (rep_user_id, collection_id) "
                    "REFERENCES epc_cli_collection(rep_user_id, collection_id) "
                    "ON DELETE CASCADE)");



/* epc_cli_collect_by_eventid table
***********************************/

repos::execute ("ALTER TABLE epc_cli_collect_by_eventid "
                    "ADD (rep_user_id NUMBER CONSTRAINT epc_cli_collect_by_eventid_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_collect_by_eventid SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_collect_by_eventid "
                    "ADD (CONSTRAINT epc_cli_collect_by_eventid_c1 "
                    "FOREIGN KEY (rep_user_id, collection_id) "
                    "REFERENCES epc_cli_collection(rep_user_id, collection_id) "
                    "ON DELETE CASCADE)");


/* epc_cli_collect_by_userid table
**********************************/

repos::execute ("ALTER TABLE epc_cli_collect_by_userid "
                    "ADD (rep_user_id NUMBER CONSTRAINT epc_cli_collect_by_userid_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_collect_by_userid SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_collect_by_userid "
                    "ADD (CONSTRAINT epc_cli_collect_by_userid_c1 "
                    "FOREIGN KEY (rep_user_id, collection_id) "
                    "REFERENCES epc_cli_collection(rep_user_id, collection_id) "
                    "ON DELETE CASCADE)");

/* epc_cli_environment_version table
************************************/

repos::execute ("ALTER TABLE epc_cli_environment_version "
                    "ADD (rep_user_id  NUMBER CONSTRAINT epc_cli_environment_version_c0 "
                    "REFERENCES epc_cli_rep_users(rep_user_id) "
                    "ON DELETE CASCADE)");

repos::execute ("UPDATE epc_cli_environment_version SET rep_user_id = :1", user_id);

repos::execute ("ALTER TABLE epc_cli_environment_version "
                    "ADD (CONSTRAINT epc_cli_environment_version_c1 "
                    "FOREIGN KEY (rep_user_id, environment_id) "
                    "REFERENCES epc_cli_environment(rep_user_id, environment_id) "
                    "ON DELETE CASCADE)");


/******************************************************************************
**  Create or replace views
**
**  The following two views support allowing the user to choose a product
**  and event set in the wizard when creating a collection.
*******************************************************************************/



repos::execute ("CREATE or REPLACE VIEW epc_cli_product "
    "(rep_user_id, node_id, environment_id, product_name, fac_num, "
     "fdf_file_status, fdf_file_active) "
    "AS "
    "SELECT DISTINCT e.rep_user_id, e.node_id, e.environment_id, f.product_name, f.fdf_file_fac_num, f.fdf_file_status, f.fdf_file_active "
        "FROM epc_cli_environment e, epc_cli_fdf_file f "
        "WHERE NOT (f.fdf_file_fac_num = 5 AND "
        "fdf_file_fac_vendor = 192216243) AND "
        "e.environment_id = f.environment_id AND "
        "e.rep_user_id = f.rep_user_id "
    "UNION "
    "SELECT DISTINCT e.rep_user_id, e.node_id, s.environment_id, s.service_name, f.fdf_file_fac_num, f.fdf_file_status, fdf_file_active "
        "FROM epc_cli_environment e, epc_cli_fdf_file f, epc_cli_service s "
        "WHERE f.fdf_file_fac_num = 5 AND "
        "fdf_file_fac_vendor = 192216243 AND "
        "e.environment_id = f.environment_id AND "
        "f.environment_id = s.environment_id AND "
        "e.rep_user_id = f.rep_user_id AND "
        "f.rep_user_id = s.rep_user_id");

repos::execute ("CREATE or REPLACE VIEW epc_cli_event_set "
    "(rep_user_id, fdf_file_id,	product_name, event_set_name, environment_id, "
     "fdf_file_desc) "
    "AS "
    "SELECT rep_user_id, fdf_file_id, product_name, event_set_name, environment_id, fdf_file_desc "
        "FROM epc_cli_fdf_file "
        "WHERE NOT (fdf_file_fac_num = 5 AND "
        "fdf_file_fac_vendor = 192216243) "
    "UNION "
    "SELECT DISTINCT f.rep_user_id, f.fdf_file_id, s.service_name, f.event_set_name, f.environment_id, f.fdf_file_desc "
        "FROM epc_cli_fdf_file f, epc_cli_service s "
        "WHERE f.fdf_file_fac_num = 5 AND "
        "fdf_file_fac_vendor = 192216243 AND "
        "f.environment_id = s.environment_id AND "
        "f.rep_user_id = s.rep_user_id");


repos::execute ("CREATE TABLE epc_tdv_version "
                "(version_product NUMBER(8,5) CONSTRAINT epc_tdv_version UNIQUE)");

/******************************************************************************
**  Update repository version
*******************************************************************************/


repos::execute ("UPDATE epc_cli_version SET version_product = 1.6");
repos::execute ("UPDATE epc_tdv_version SET version_product = 1.6");

