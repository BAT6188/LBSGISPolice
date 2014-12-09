/* Copyright(c) Oracle Corporation 1999. All rights reserved */

/**
 *      NAME
 *          VdkUpgrade210.xrl
 *
 *      PUBLIC CLASSES
 *          VdkUpgrade210
 *
 *      NOTES
 *
 *          This class will upgrade all Oracle Expert repository objects.
 *
 *  @author  Greg Smith
 */

int s1,s2,i1,i2,i3,i4,i5;
String buf,buf2,buf3,d1,d2;


/*******************************************************************************
**  Move vdk_sql
*******************************************************************************/

s1 = OpenStream( "SELECT s2.xp_text, s1.xp_sql_id " 
                 "FROM xp_sql s1, xp_shared_statement s2 " 
                 "WHERE s1.xp_sql_hash_id = s2.xp_id");

while (Fetch (s1, buf, i1))
  {
    ExecuteSql("UPDATE vdk_sql "
               "SET vdk_text = :1 " 
               "WHERE vdk_sql_id = :2 ",
               buf, i1);
  }

CloseStream(s1);

Commit();

            
/*******************************************************************************
**  Set the sequence for vdk_journal_seq
*******************************************************************************/
      
i1 = SelectInteger("SELECT xp_journal_seq.nextval FROM dual");

ExecuteSql("DROP SEQUENCE vdk_journal_seq");
            
buf = "create sequence vdk_journal_seq start with ";
          
strcat(buf, i1);
strcat(buf, " increment by 1 nominvalue maxvalue ");
strcat(buf, i1 + 999999999);
strcat(buf, " order cycle cache 50");
          
ExecuteSql (buf);
            
Commit();

/*******************************************************************************
**  Set control sequence
*******************************************************************************/
      
i1 = SelectInteger("SELECT xp_rep_control_sequence.nextval FROM dual");

ExecuteSql("DROP SEQUENCE vdk_rep_control_sequence");
            
buf = "create sequence vdk_rep_control_sequence start with ";
          
strcat(buf, i1);
strcat(buf, " increment by 1 nominvalue nomaxvalue ");
strcat(buf, " nocycle cache 500");

ExecuteSql(buf);
          
Commit();

/*******************************************************************************
**  Set the sql id sequence
*******************************************************************************/
      
i1 = SelectInteger("SELECT xp_sql_id_sequence.nextval FROM dual");

ExecuteSql("DROP SEQUENCE vdk_sql_id_sequence");
            
buf = "create sequence vdk_sql_id_sequence start with ";
          
strcat(buf, i1);
strcat(buf, " increment by 1 nominvalue nomaxvalue ");
strcat(buf, " nocycle cache 500");
            
ExecuteSql(buf);

Commit();

/*******************************************************************************            
**  Update vdk_object
*******************************************************************************/

int STAT_ORIGINAL = 1 << 11;
int STAT_COPIED = 1 << 21;
int STAT_NEW = 1 << 3;
int STAT_CHANGED = 1 << 4;
int STAT_DELETED = 1 << 5;
int STAT_INCOMPLETE = 1 << 8;
int STAT_INVALID = 1 << 0;
int STAT_COLLECT = 1 << 14;
int STAT_EXISTS = 1 << 15;

int creationStatus, workingStatus, incomplete, invalid, collect;
int udr_exists, goodType;

s1 = OpenStream("SELECT obj.xp_id, xp_parent, xp_type, xp_name, "
                "TO_CHAR(xp_mod_date,"
                    "'MM/DD/YYYY HH24:MI:SS',"
                    "'NLS_DATE_LANGUAGE=American'"
                   "),"
                "TO_CHAR(xp_mod_date,"
                    "'MM/DD/YYYY HH24:MI:SS',"
                    "'NLS_DATE_LANGUAGE=American'"
                    "),"
                "xp_hidden, xp_status, xp_ancestry FROM xp_object obj, xp_ancestry anc "
                "WHERE obj.xp_id = anc.xp_id and xp_name not like 'COPY\\_%' ESCAPE '\\'");

while (Fetch(s1, i1, i2, i3, buf, d1, d2, i4, i5, buf2))
  {
    goodType = true;
            
    creationStatus = workingStatus = incomplete = 0;
    invalid = collect = udr_exists = 0;

    switch (i3)
      {
        case 300:
        case 301: 
        case 302: 
        case 303: 
        case 304: 
        case 306:
        case 9001:
          goodType = false;
          break;
        
        case 320:
          i3 = 202;
          break;
        
        case 399:
          i3 = 235;
          break;                  
        case 705:
          i3 = 102;
          strcpy(buf, "SQL History");
          break;    

      }
           
    if (!goodType)
      {
        continue;
      }

    if(i5 & STAT_INCOMPLETE)
      incomplete = 1;
    if(i5 & STAT_INVALID)
      invalid = 1;
    if(i5 & STAT_COLLECT)
      collect = 1;
    if(i5 & STAT_EXISTS)
      udr_exists = 1;
         
    ExecuteSql("INSERT INTO vdk_object (VDK_ID,VDK_PARENT_ID,VDK_TYPE,"
               "VDK_NAME,VDK_ANCESTRY_IDS,VDK_CRE_DATE,VDK_MOD_DATE,"
               "VDK_CREATION_STATUS,"
               "VDK_WORKING_STATUS,VDK_IS_HIDDEN,VDK_IS_INCOMPLETE,"
               "VDK_IS_INVALID,VDK_IS_COLLECT,VDK_UDR_EXISTS) "
               "VALUES(:1, :2, :3, :4, :5,"
               "TO_DATE(:6,'MM/DD/YYYY HH24:MI:SS',"
                  "'NLS_DATE_LANGUAGE=American'),"
               "TO_DATE(:7,'MM/DD/YYYY HH24:MI:SS',"
                  "'NLS_DATE_LANGUAGE=American'),"
               ":8, :9, :10, :11, :12, :13, :14)",
               i1,i2,i3,buf,buf2,d1,d2,
               creationStatus, workingStatus,
               i4,incomplete,invalid,collect,
               udr_exists);
  }

CloseStream(s1);

Commit();


/*******************************************************************************
**  Fix up vdk_synonyms
*******************************************************************************/

s1 = OpenStream("SELECT vdk_id, vdk_parent_id, vdk_table_name FROM vdk_synonym ");

while (Fetch(s1,i1,i2,buf))
  {
    s2 = OpenStream("SELECT vdk_id, vdk_type FROM vdk_object WHERE vdk_parent_id = :1 "
                    " AND (vdk_type = 401 OR vdk_type = 407) AND vdk_name = :2",i2,buf);

    while (Fetch(s2, i3, i4))
      {
        ExecuteSql("UPDATE vdk_synonym SET vdk_reference_id = :1, "
                   "vdk_reference_type = :2 WHERE vdk_id = :3",
                   i3,i4,i1);

      }
  
    CloseStream(s2);
  }

CloseStream(s1);

Commit();



/*****************************************************************************************
 * Upgrade the vdk_index table flags field.  Done in Java because of the bit-wise operations (etc.).
 ***************************************************************************************/

int indexId, oldFlags;
int newFlags = 0;
int OLD_LOCKED = 1 << 1;
int OLD_TEMP = 1 << 3;
int OLD_WIP = 1 << 4;
int OLD_CONSTRAINT = 1 << 5;
int OLD_MATCHES_CONSTRAINT  = 1 << 6;
int OLD_FK = 1 << 9;
int OLD_ASSOCIATED_IOT = 1 << 10;
int OLD_XP_STATS = 1 << 15;
int OLD_ANA_STATS = 1 << 16;
int OLD_READ_STATS = 1 << 17;
int OLD_FULL   = 1 << 18;
int OLD_EST = 1 << 19;
int OLD_SCHEMA_DATA = 1 << 20;
int OLD_NO_SAVE  = 1 << 21;
int NEW_LOCKED  = 1 << 0;
int NEW_TEMP = 1 << 1;
int NEW_WIP  = 1 << 2;
int NEW_CONSTRAINT = 1 << 3;
int NEW_MATCHES_CONSTRAINT  = 1 << 4;
int NEW_FK  = 1 << 5;
int NEW_ASSOCIATED_IOT = 1 << 6;
int NEW_XP_STATS  = 1 << 13;
int NEW_ANA_STATS  = 1 << 14;
int NEW_READ_STATS = 1 << 15;
int NEW_FULL  = 1 << 16;
int NEW_EST  = 1 << 17;
int NEW_SCHEMA_DATA  = 1 << 18;
int NEW_NO_SAVE  = 1 << 19;

 s1 = OpenStream("SELECT vdk_id, vdk_flags FROM vdk_index");
 while (Fetch(s1, indexId, oldFlags))
   {

     if((oldFlags & OLD_LOCKED) != 0)
        newFlags |= NEW_LOCKED;

     if((oldFlags & OLD_CONSTRAINT) != 0)
        newFlags |= NEW_CONSTRAINT;

     if((oldFlags & OLD_MATCHES_CONSTRAINT) != 0)
         newFlags |= NEW_MATCHES_CONSTRAINT;

             
     ExecuteSql("UPDATE vdk_index SET vdk_flags = :1 WHERE vdk_id = :2",
                   newFlags, indexId);


   }

        
CloseStream(s1);

Commit();


/*******************************************************************************
**  Drop old objects
*******************************************************************************/

s1 = OpenStream("SELECT object_type, object_name FROM user_objects WHERE "
                "object_type <> 'INDEX' AND object_name LIKE 'XP_%' OR object_name LIKE 'SMP_TBLSP_%'");

while (Fetch(s1,buf,buf2))
  {
    sprintf(buf3,"DROP %1 %2",buf,buf2);
    ExecuteSql_Ignore(buf3);
  }

CloseStream(s1);

ExecuteSql_Ignore("delete from smp_vds_repos_version s where s.app_name = 'VMT'");
ExecuteSql_Ignore("delete from smp_vds_repos_version s where s.app_name = 'SQL Analyze'");
