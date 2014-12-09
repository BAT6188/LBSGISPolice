/* Copyright(c) Oracle Corporation 1999. All rights reserved */

/**
 *      NAME
 *          OCMLongsMigrate.xrl
 *
 *      NOTES
 *
 *          This file is the migration script for migrating OCM 2.0.4 tables that
 *          contain longs.
 *
 *         This file should be run with others in the following order:
 *         ocm21rn_migrate.sql
 *         ocm21cre_migrate.sql
 *         ocm21pop1_migrate.sql
 *         OCMLongsMigrate.xrl
 *         ocm21pop3_migrate.sql
 *         ocm21dr_migrate.sql
 *
 */

xp_int_t cur, i1, i2, i3, i4, i5;
xp_str_t s2, s3, s4, s5, d1, str_buf;
raw  raw_buf;

/*******************************************************************************
**  move change plans
*******************************************************************************/
printf(get_text(xprc_gl_wip_mig_ocm_info2), "vbz$change_plans");
cur = target::open( " SELECT cp_id, "
                 "cp_user_name, "
                 "cp_name, "
                 "cp_source_db, "
                 "cp_description, "
                 "TO_CHAR(cp_create_date,"
                    "'MM/DD/YYYY HH24:MI:SS',"
                    "'NLS_DATE_LANGUAGE=American'"
                   "),"
                 "cp_options, "
                 "cp_type, "
                 "cp_scope "
		         " FROM vbzold$change_plans ");
while (target::fetch (&cur, &i1, &s2, &s3, &s4, &s5, &d1, &i2, &i3, &raw_buf))
  {
 	target::execute(" INSERT INTO vbz$change_plans ( cp_id, cp_user_name, cp_name, cp_source_db, "
                             " cp_description, cp_create_date, cp_options, cp_type, cp_scope, "
                             " cp_latest_version_no ) "
               "VALUES(:1, :2, :3, :4, :5, "
                       "TO_DATE(:6,'MM/DD/YYYY HH24:MI:SS',"
                       "'NLS_DATE_LANGUAGE=American'),"
                       ":7, :8, :9, :10)",
                   i1, s2, s3, s4, s5, d1, i2, i3, &raw_buf, 1);
  }

target::close(&cur);

/*******************************************************************************
**  move DbObjNames
*******************************************************************************/

printf(get_text(xprc_gl_wip_mig_ocm_info2), "vbz$db_obj_names");
cur = target::open(" SELECT cp_id, db_obj_id, db_obj_type, db_obj_name, "
                "db_obj_schema, db_obj_scope " 
		        " FROM vbzold$db_obj_names ");
while (target::fetch (&cur, &i1, &i2, &i3, &s2, &s3, &raw_buf))
  {

 	target::execute(	" INSERT INTO vbz$db_obj_names ( cp_id, db_obj_id, db_obj_type, db_obj_name, db_obj_schema, db_obj_scope ) " 
	            "VALUES(:1, :2, :3, :4, :5, :6)",
                   i1, i2, i3, s2, s3, &raw_buf);
  }

target::close(&cur);

/*******************************************************************************
**  move Directives
*******************************************************************************/

printf(get_text(xprc_gl_wip_mig_ocm_info2), "vbz$directives");
cur = target::open( " SELECT cp_id, db_obj_id, def_length, definition " 
		         " FROM vbzold$directives ");
while (target::fetch (&cur, &i1, &i2, &i3, &raw_buf))
  {

 	target::execute(	"INSERT INTO vbz$directives ( cp_id, db_obj_id, def_length, version_no, flags, definition ) "
	            "VALUES(:1, :2, :3, :4, :5, :6)",
                   i1, i2, i3, 1, 0, &raw_buf);
  }

target::close(&cur);

/*******************************************************************************
**  move Exemplars
*******************************************************************************/

printf(get_text(xprc_gl_wip_mig_ocm_info2), "vbz$exemplars");
cur = target::open( " SELECT cp_id, db_obj_id, options, def_length, definition "
		         " FROM vbzold$exemplars ");
while (target::fetch (&cur, &i1, &i2, &i3, &i4, &raw_buf))
  {

 	target::execute(	" INSERT INTO vbz$exemplars ( cp_id, db_obj_id, options, def_length, version_no, flags, definition ) "
	            " VALUES(:1, :2, :3, :4, :5, :6, :7)",
                   i1, i2, i3, i4, 1, 0, &raw_buf);
  }

target::close(&cur);


/*******************************************************************************
**  moveDestinations
*******************************************************************************/

printf(get_text(xprc_gl_wip_mig_ocm_info2), "vbz$destinations");
cur = target::open( " SELECT D.CP_ID, D.DB_NAME, D.DEST_DESCRIPTION, C.CP_OPTIONS, D.SCRATCH_TS, D.TRANS_STATUS, D.STATUS, FLAGS "
                 " FROM VBZOLD$DESTINATIONS D, VBZOLD$CHANGE_PLANS C "
                 " WHERE D.CP_ID = C.CP_ID " );
while (target::fetch (&cur, &i1, &s2, &s3, &i2, &s4, &i3, &i4, &i5))
  {

 	target::execute(  " insert /*+ APPEND */ into VBZ$DESTINATIONS ( CP_ID, DB_NAME, DEST_DESCRIPTION, OPTIONS, SCRATCH_TS, VERSION_NO, DEPLOYMENT_ID, TRANS_STATUS, TRANS_HISTORY_ID, EXEC_STATUS, EXEC_HISTORY_ID, FLAGS ) "
                 " VALUES(:1, :2, :3, :4, :5, 1, VBZ$REP_OBJ_ID_SEQ.NEXTVAL, :6, 1, :7, VBZ$HISTORY_SEQ.NEXTVAL, :8)",
                   i1, s2, s3, i2, s4, i3, i4, i5);
  }

target::close(&cur);


/*******************************************************************************
**  moveOutputLog
*******************************************************************************/

printf(get_text(xprc_gl_wip_mig_ocm_info2), "vbz$output_log");
cur = target::open( " SELECT deployment_id, log_line_no, exec_history_id, o.log_line "
		         " FROM vbzold$outputlog o, vbz$destinations d "
		         " WHERE d.cp_id = o.cp_id AND d.db_name = o.db_name ");
while (target::fetch (&cur, &i1, &i2, &i3, &str_buf))
  {

 	target::execute(	" INSERT INTO vbz$output_log ( deployment_id, log_line_no, history_id, log_line ) " +
		        " VALUES(:1, :2, :3, :4)",
                   i1, i2, i3, str_buf);
  }

target::close(&cur);

/*******************************************************************************
**  moveScripts
*******************************************************************************/

printf(get_text(xprc_gl_wip_mig_ocm_info2), "vbz$scripts");
cur = target::open( " SELECT deployment_id, script_line_no, script_step_no, script_line_type, script_section, script_line "
		         " FROM vbzold$scripts s, vbz$destinations d "
		         " WHERE d.cp_id = s.cp_id AND d.db_name = s.db_name ");
while (target::fetch (&cur, &i1, &i2, &i3, &s2, &s3, &str_buf))
  {

 	target::execute( " INSERT INTO vbz$scripts ( deployment_id, script_line_no, script_step_no, script_line_type, script_section, script_line ) "
		        " VALUES(:1, :2, :3, :4, :5, :6)",
                   i1, i2, i3, s2, s3, str_buf);
  }

target::close(&cur);

/*******************************************************************************
**  moveEditedScripts
*******************************************************************************/

printf(get_text(xprc_gl_wip_mig_ocm_info2), "vbz$edited_scripts");
cur = target::open( " SELECT deployment_id, script_length, script_content "
		         " FROM vbzold$edited_scripts e, vbz$destinations d "
		         " WHERE d.cp_id = e.cp_id AND d.db_name = e.db_name ");
while (target::fetch (&cur, &i1, &i2, &str_buf))
  {

 	target::execute( " INSERT INTO vbz$edited_scripts ( deployment_id, script_length, script_content ) " +
                " VALUES(:1, :2, :3)",
                   i1, i2, str_buf);
  }

target::close(&cur);

