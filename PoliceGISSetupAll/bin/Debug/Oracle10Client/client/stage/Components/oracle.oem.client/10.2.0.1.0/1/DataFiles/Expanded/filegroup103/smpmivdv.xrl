/* Copyright (c) Oracle Corporation, 1997, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpmivdv.xrl
*
*   DESCRIPTION
*
*       Migration script for the OEM V2 repository vdv (preferred credentials) 
*       tables.  
*
*   NOTES
*
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t owner, user_name, enc_password, service_type, service_name, dec_password;
xp_str_t role;
xp_int_t s1, user_id;

printf(get_text(xprc_gl_wip_mig_vdv_head));

/***************************************************************************
 **  Migrate the smp_vdv_user table.
 ***************************************************************************/

if (!target::integer ("select count(*) from smp_vdv_user "
                         "where user_name = :1", v2user))
  {
    target::execute("insert into smp_vdv_user values "
             "(smp_vdv_user_id_sequence.nextval, :1, :2)", v2user, v2user);
  }

/***************************************************************************
 **  Migrate the smp_vdv_general table.
 ***************************************************************************/

target::select("select user_id from smp_vdv_user where "
                        "user_name = :1", 1, 1, v2user, &user_id);

if (!target::integer ("select count(*) from smp_vdv_general "
                         "where user_id = :1", user_id))
      {
        target::execute("insert into smp_vdv_general values "
                       "( :1, 0, '','',1)", user_id);
      }

/***************************************************************************
 **  Migrate the smp_vdv_preferred_credentials table.
 ***************************************************************************/

s1 = repos::open ("select * from v2_smp_vdv_migrate_1");

while (repos::fetch (&s1, owner, service_type, service_name, user_name, enc_password, role))
  {

    repos::xp_decrypt_pwd(owner, enc_password, dec_password);

    if (service_type == "oracle_sysman_node")
      {
        service_name = repos::gethostname(service_name);
      }

    if (!target::integer ("select count(*) from smp_vdv_preferred_credentials "
                          "where user_id = :1 and service_type = :2 "
                          "and service_name = :3", user_id, service_type, service_name))
      {
        target::execute("insert into smp_vdv_preferred_credentials "
                        "(user_id, service_type, service_name) "
                        "values ( :1, :2, :3)", user_id, service_type, service_name);

        repos::store_credentials(v2domain, user_name, dec_password, user_id, 
                                          role, service_type, service_name);
      }
    else
      printf(get_text(xprc_gl_wip_mig_vdv_info2), v2user, service_name);
  }

repos::close(&s1);


/***************************************************************************
 **  If there are users already existing in smp_vdv_user table 
 **  then we need to insert default rows for the notification filter
 **  for those users.
 ***************************************************************************/

s1 = target::open ("SELECT	smp_vdv_user.user_id
					FROM	SMP_VDV_USER
					WHERE	SMP_VDV_USER.user_id NOT IN
						(SELECT smp_vdv_notification_state.user_id 
						 FROM smp_vdv_notification_state)");

while (target::fetch (&s1, &user_id))
  {

    target::execute("insert into smp_vdv_notification_state "
                        "(user_id, event_opstate_notify, job_opstate_notify) "
                        "values ( :1, -1, -1)", user_id);
  }

target::close(&s1);

printf(get_text(xprc_gl_wip_mig_vdv_tail));