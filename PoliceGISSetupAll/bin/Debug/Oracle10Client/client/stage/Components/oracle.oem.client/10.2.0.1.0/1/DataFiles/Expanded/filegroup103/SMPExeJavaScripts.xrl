/* Copyright(c) Oracle Corporation 1999. All rights reserved */

/**
 *      NAME
 *          ExeJavaScripts.xrl
 *
 *      NOTES
 *
 *        	This file is for executing java scripts used by EMCA for upgrading repository through JNI
 */

/**********************************************************************************/

xp_str_t result;
    
result = exe_oem_java_scripts();

//printf("result: %1", result);
printf(get_text(xprc_gl_wip_mig_vde_tail));

     
   