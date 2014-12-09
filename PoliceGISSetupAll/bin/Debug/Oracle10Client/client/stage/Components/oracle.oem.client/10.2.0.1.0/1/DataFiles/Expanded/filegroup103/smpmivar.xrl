/* Copyright (c) Oracle Corporation, 1998, 2000.  All Rights Reserved.    */

/*******************************************************************************
*
*   NAME
*
*       smpmivar.xrl
*
*   DESCRIPTION
*
*       Migration script for the OEM V2 repository var (backup manager) tables.  
*
*   NOTES
*
*
*******************************************************************************/

/*******************************************************************************
**  Local variables
*******************************************************************************/

xp_str_t channelname, formatstring, formatdest, channeldevice, devicename;
xp_str_t devicetype, parameters, limitsize, readrate, openfiles, dbname;
xp_str_t oemusername, jobid, targetdb, jobname, jobdescription, jobtype;
xp_str_t tclscript, rmanscript, userc, username, service;
xp_int_t s1, s2, s3;

/***************************************************************************
**  Migrate the channel information.
***************************************************************************/

s1 = repos::open ("select * from SMP_BRM_CHANNEL_DEVICE_");

while (repos::fetch (&s1, &channelname, &formatstring, &formatdest,
			&channeldevice, &devicename, &devicetype, &parameters,
			&limitsize, &readrate, &openfiles, &dbname))
  {
    if (!target::integer ("select count(*) from  SMP_VAR_SMR_CHANNEL_DEVICE_ "
                         "where oemusername = :1 and dbname = :2 and "
						 "channelname = :3", v2user, dbname, channelname))
      {
        target::execute("insert into SMP_VAR_SMR_CHANNEL_DEVICE_ values "
                       "(:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12)",
                        channelname, formatstring, formatdest, channeldevice,
						devicename, devicetype, parameters, limitsize, 
						readrate, openfiles, dbname, v2user);
      }
  }
repos::close(&s1);

s1 = repos::open ("select * from SMP_BRM_DEFAULT_CHANNEL_");

while (repos::fetch (&s1, &channelname, &dbname))
  {
    if (!target::integer ("select count(*) from  SMP_VAR_SMR_DEFAULT_CHANNEL_ "
                         "where oemusername = :1 and dbname = :2", v2user, 
						 dbname))
      {
        target::execute("insert into SMP_VAR_SMR_DEFAULT_CHANNEL_ values "
                       "(:1, :2, :3)", channelname, dbname, v2user);
      }
  }
repos::close(&s1);

/***************************************************************************
 **  Migrate the connect string information.
 ***************************************************************************/

s1 = repos::open ("select dbname, username, service from SMP_BRM_RC_CONNECT_STRING_");

while (repos::fetch (&s1, &dbname, &username, &service))
  {
    if (!target::integer ("select count(*) from SMP_VAR_SMR_RC_CONNECT_STRING_ "
                         "where oemusername = :1 and dbname = :2", v2user, 
						 dbname))
      {
        target::execute("insert into SMP_VAR_SMR_RC_CONNECT_STRING_ (dbname, "
						"username, service, oemusername) values "
                       "(:1, :2, :3, :4)", dbname, username, service, v2user);
		    repos::migrate_var_password( v2user, dbname );
      }
  }

repos::close(&s1);

/***************************************************************************
 **  Migrate the SMR job information.
 ***************************************************************************/

s1 = repos::open ("select * from SMP_BRM_ACTIVE_JOB_");

while (repos::fetch (&s1, &jobid, &targetdb, &jobname))
  {
    if (!target::integer ("select count(*) from SMP_VAR_SMR_ACTIVE_JOB_ "
                         "where oemusername = :1 and jobid = :2", v2user, 
                          jobid))
      {
        target::execute("insert into SMP_VAR_SMR_ACTIVE_JOB_ values "
                       "(:1, :2, :3, :4)", jobid, targetdb, jobname, 
						v2user);

		    s2 = repos::open ("select * from SMP_BRM_TEMP_SCRIPTS_ where "
						    "jobid = :1", jobid);

		    while (repos::fetch (&s2, &jobid, &tclscript, &rmanscript))
		      {
			      target::execute("insert into SMP_VAR_SMR_TEMP_SCRIPTS_ values "
						         "(:1, :2, :3, :4)", jobid, tclscript, 
							      rmanscript, v2user);
		      }
		    repos::close(&s2);

		    s3 = repos::open ("select * from SMP_BRM_SAVED_JOB_ where jobname = :1 ",
							            targetdb);

		    while (repos::fetch (&s3, &jobname, &jobdescription, &dbname, &jobtype))
		      {
			      if (!target::integer ("select count(*) from SMP_VAR_SMR_SAVED_JOB_ "
								       "where oemusername = :1 and jobname = :2", v2user, 
								       jobname))
			        {
				        target::execute("insert into SMP_VAR_SMR_SAVED_JOB_ values "
							           "(:1, :2, :3, :4, :5)", jobname, jobdescription, 
								        dbname, jobtype, v2user);
				        
			        }
		      }
		    repos::close(&s3);
      }
  }

repos::close(&s1);

/***************************************************************************
 **  Migrate the OS job information.
 ***************************************************************************/

s1 = repos::open ("select * from SMP_VAB_ACTIVE_JOB_");

while (repos::fetch (&s1, &jobid, &targetdb, &jobname))
  {
    if (!target::integer ("select count(*) from SMP_VAR_OS_ACTIVE_JOB_ "
                         "where oemusername = :1 and jobid = :2", v2user, 
                         jobid))
      {
        target::execute("insert into SMP_VAR_OS_ACTIVE_JOB_ values "
                       "(:1, :2, :3, :4)", jobid, targetdb, jobname, 
						            v2user);

		    s2 = repos::open ("select * from SMP_VAB_SAVED_JOB_ where jobname = :1",
				                  targetdb);

		    while (repos::fetch (&s2, &jobname, &jobdescription, &dbname))
		      {
			      if (!target::integer ("select count(*) from SMP_VAR_OS_SAVED_JOB_ "
								       "where oemusername = :1 and jobname = :2", v2user, 
								       jobname))
			        {
				        target::execute("insert into SMP_VAR_OS_SAVED_JOB_ values "
							           "(:1, :2, :3, :4)", jobname, jobdescription, 
								        dbname, v2user);
			        }
		      }

		    repos::close(&s2);
      }
  }

repos::close(&s1);

/***************************************************************************
 **  Migrate the EBU job information.
 ***************************************************************************/

s1 = repos::open ("select * from SMP_EBU_ACTIVE_JOB_");

while (repos::fetch (&s1, &jobid, &targetdb, &jobname))
  {
    if (!target::integer ("select count(*) from SMP_VAR_EBU_ACTIVE_JOB_ "
                         "where oemusername = :1 and jobid = :2", v2user, 
                         jobid))
      {
        target::execute("insert into SMP_VAR_EBU_ACTIVE_JOB_ values "
                       "(:1, :2, :3, :4)", jobid, targetdb, jobname, 
                        v2user);

		    s2 = repos::open ("select * from SMP_EBU_SAVED_JOB_ where jobname = :1",
							    targetdb);

		    while (repos::fetch (&s2, &jobname, &jobdescription, &dbname))
		      {
			      if (!target::integer ("select count(*) from SMP_VAR_EBU_SAVED_JOB_ "
								       "where oemusername = :1 and jobname = :2", v2user, 
								       jobname))
			        {
				        target::execute("insert into SMP_VAR_EBU_SAVED_JOB_ values "
							           "(:1, :2, :3, :4)", jobname, jobdescription, 
								        dbname, v2user);
			        }
		      }
		    repos::close(&s2);
      }
  }

repos::close(&s1);

/***************************************************************************
 **  Migrate the list of database.
 ***************************************************************************/

s1 = repos::open ("select * from SMP_VAR_LIST_DATABASES_");

while (repos::fetch (&s1, &dbname, &userc))
  {
    if (!target::integer ("select count(*) from SMP_VAR_SMR_LIST_DATABASES_ "
                         "where dbname = :1", dbname))
      {
        target::execute("insert into SMP_VAR_SMR_LIST_DATABASES_ values "
                       "(:1, :2, :3)", dbname, userc, v2user);
      }
  }

repos::close(&s1);
