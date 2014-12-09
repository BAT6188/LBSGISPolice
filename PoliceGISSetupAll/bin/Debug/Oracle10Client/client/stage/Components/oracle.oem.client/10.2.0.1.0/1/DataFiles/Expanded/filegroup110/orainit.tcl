# $Header: orainit.tcl 23-oct-2003.16:49:37 ngade Exp $
#
# Copyright (c) 1995, 2003, Oracle Corporation.  All rights reserved.  
#
# NAME:
#
#  orainit.tcl : This file contains the portable definitions and procedures
#                used by all tcl scripts. 
#
# $Log:  $
# Revision 1.9  1996/02/28  20:16:45  yliu
# Fixed parsing problem in convertin and out
#
# Revision 1.8  1996/02/07 18:36:52  yliu
# added convertin and convertout
#
# Revision 1.7  1996/01/11 00:41:25  yliu
# added msgtxt1
#
# Revision 1.6  1996/01/10 00:02:19  yliu
# added msgtxt command
#
# Revision 1.5  1995/09/27 18:50:18  yliu
# added global lists
#
# Revision 1.4  1995/09/14  23:01:19  yliu
# Added divide procedure
#
#

# Global definitions
set SCRIPT_FAIL -2
set CLEAR_EVENT -1
set NO_EVENT 0
set WARNING_EVENT 1
set ALERT_EVENT 2
set SYS_TYPE sys
set TOTAL_TYPE total
set USED_TYPE used
set CHUNK_TYPE chunk
set IO_TYPE io
set TIME_STAMP timestamp
set month_list {Jan Feb Mar Apl May Jun Jul Aug Sep Oct Nov Dec}
set sysstat_name {lib_cache dc_cache buf_cache physical_io sort table_scan user_logged_on active_users sga_size uga_size}
set nmi_trace_info ""


# Set the prompt for oratclsh
set tcl_prompt1 PROMPT
proc PROMPT {} {
    puts -nonewline "oratclsh\[[history nextid]\]- "
    flush stdout
}


proc get_oids {base end} {
    # Gets value of all oids btwn base and end and puts them in a list
    # Basically it is like a get-bulk operation
    set db_ret [oradbsnmp getnext $base]
    set oid [lindex $db_ret 0]
    set end_oid [lindex [oradbsnmp getnext $end] 0]
    set ret ""
    while {[string compare $oid $end_oid] < 0} {
	lappend ret [lindex $db_ret 1]
	set db_ret [oradbsnmp getnext $oid]
	set oid [lindex $db_ret 0]
    }
    return $ret
}


proc divide {a b} {
    # Divide a by b, filter out divide by zero condition
    if {$b == 0} {
        expr 0
    } else {
        expr $a * 1.0 / $b
    }
}


proc msgtxt {product facility error} {
    # Get message text given product, facility and message number
    # The output is in the format of "FACILITY-ERROR : MESSAGE TEXT"
    set msgid [openmsg $product $facility]
    set text "$facility-$error: [getmsg $msgid $error]"
    closemsg $msgid
    return $text
}


proc msgtxt1 {product facility error} {
    # Get message text given product, facility and message number
    # The output is in the format of "MESSAGE TEXT"
    set msgid [openmsg $product $facility]
    set text [getmsg $msgid $error]
    closemsg $msgid
    return $text
}


proc convertin {agent_characterset string} {
    # Convert character set for incoming string
    global oramsg
    regexp {([A-Za-z_ ]+).([A-Za-z0-9]+)} $oramsg(language) a b c
    set cvtstring [cvtcharset $c $agent_characterset $string]
    return $cvtstring
}


proc convertout {agent_characterset string} {
    # Convert character set for outgoing string
    global oramsg
    regexp {([A-Za-z_ ]+).([A-Za-z0-9]+)} $oramsg(language) a b c
    set cvtstring [cvtcharset $agent_characterset $c $string]
    return $cvtstring
}


proc getlsnrinfo {name} {
    # returns the long name, listener.ora file and shortname of this listener
    set conffn ""
    set lname ""
    set sname ""
    set SnmproHdl [open [SNMPRO_FILE] r];
    while {[gets $SnmproHdl line] >= 0} {
	if {[regexp -nocase "snmp.shortname.$name *=" $line] } {
	    set sname [string trim [string range $line [expr [string first = $line] +1] end] " "]
	}
	if {[regexp -nocase "snmp.longname.$name *=" $line] } {
	    set lname [string trim [string range $line [expr [string first = $line] +1] end] " "]
	}
	if {[regexp -nocase "snmp.configfile.$name *=" $line] } {
	    set conffn [string trim [string range $line [expr [string first = $line] +1] end] " "]
	}
    }
    close $SnmproHdl
    return [list $lname $conffn $sname]
}


proc getlsnrstatus {name} {
    # returns the status of the listener (1=up, 0=down)
    global tcl_platform

    if { [info exists env(ORACLE_HOME)] } {
      set oldOracleHome $env(ORACLE_HOME)
    }
    set env(ORACLE_HOME) [lindex [getlsnrdata $name] 3]

    set ret_code [catch {exec [LSNRCTL $name] ifile=[lindex [getlsnrinfo $name] 1] status [lindex [getlsnrinfo $name] 2]} buffer]
    
    if { [info exists oldOracleHome] } {
      set env(ORACLE_HOME) $oldOracleHome
    } else {
      unset env(ORACLE_HOME)
    }

    if { $ret_code == 1 || [regexp "TNS-12" $buffer] } {
       return 0
    } else {
       return 1
    }
}


proc getagenttraceinfo {} {
    # return the trace level and directory
    global nmi_trace_info
    if { $nmi_trace_info == "" } {
	set level 0
	set dir [NETWORK_TRACE_DIR]
	set SnmproHdl [open [SNMPRW_FILE] r];
	while {[gets $SnmproHdl line] >= 0} {
	    if {[regexp dbsnmp.trace_level $line] } {
          if {[regexp ^dbsnmp.trace_level [string trimleft $line]] } {
              set level [string trim [string range $line \
                        [expr [string first = $line] +1] end] " "]
          }
      }
	    if {[regexp dbsnmp.trace_directory $line] } {
          if {[regexp ^dbsnmp.trace_directory [string trimleft $line]] } {
              set dir [string trim [string range $line \
                      [expr [string first = $line] +1] end] " "]
          }
	    }
	}
	set nmi_trace_info [list $level $dir]
	close $SnmproHdl
    }
    return $nmi_trace_info 
}


proc oraagent args {
    # Get the current state of the agent
    # possible arguments are: "", "services", "users", "jobs", "events", 
    # "job_notifications 'username'", "event_reports 'username'"  
    # "user 'username'", "job 'jobID'" and "event 'eventID'"
    set ArgsLen [llength $args]
    set element [string toupper [lindex $args 0]];
    set elementArg [lindex $args 1];
    set elementArg2 [lindex $args 2];
    set usage "Usage: oraagent <services> || <users> || <jobs> || <events> || <blackouts> || <job_notifications username> || <event_reports username> || <service servicename> || <user username> || <job jobID> || <event eventID> || <blackout blackoutID> || <job_notification username notification_id> || <event_report username report_id>";
    if { $ArgsLen > 3 } {
	error $usage;
    } elseif { $ArgsLen == 0 } {
	set DbVerHdl [open [DBSNMPVER_FILE] r];
	while {[gets $DbVerHdl line] >= 0} {
	    if {[regexp product $line] } {
		puts "Version Banner:   [string range $line 9 [expr [string length $line] - 2] ]";
	    }
	}
	close $DbVerHdl;
	puts "Current State:    [lindex [getagentstatus] 1]"
	puts "Trace Level:      [lindex [getagenttraceinfo] 0]"
	puts "Trace Directory:  [lindex [getagenttraceinfo] 1]"
	return ""
    } elseif { $element == "START"} {
	puts [lindex [startagent] 1]
    } elseif { $element == "STOP"} {
	puts [lindex [stopagent] 1]
    } elseif { $element == "SERVICES"} {
	set srvlist ""
	set SHdl [open [SERVICES_FILE] r];
	while {[gets $SHdl line] >= 0} {
	    if {[regexp "=" $line] } {
		lappend srvlist [string trim [lindex [split $line =] 0] " "]
	    }
	}
	close $SHdl
	return [lsort $srvlist]
    } elseif { $element == "SERVICE" && $elementArg != "" } {
	set found  0
	set Name $elementArg;
	set SHdl [open [SERVICES_FILE] r];
	while {[gets $SHdl line] >= 0} {
	    if {[regexp "^$Name =" $line] } {
		set found 1
		regsub -all " " [string trim [string range $line [expr [string first = $line] +1] end] "() "] "" service_descr
		set service_descr_attr [split $service_descr ","]
		puts "Name:             $Name"
		puts "Type:             [lindex $service_descr_attr 0]"
		puts "Address:          [lindex $service_descr_attr 2]" 
	    }
	}
	close $SHdl
	if { $found == 0 } {
	    error "Unknown Service: $Name"
	}
    } elseif { $element == "USERS"} {
	set x [admin_listinputqueue users]
	upvar $x y
	return [lsort [array names y]];
    } elseif { $element == "USER" && $elementArg != "" } {
        set userlist [admin_listinputqueue users]
	upvar $userlist theArray
	set Name $elementArg;
        if { [catch {set test [lindex $theArray($Name) 1]}] } {
	    puts "Uknown user: $Name"
	} else {
	    puts "User Name:        $Name";
	    puts "Console Address:  [lindex $theArray($Name) 1]";
	    puts "User Language:    [lindex $theArray($Name) 2]";
	}
    } elseif { $element == "JOBS"} {
	set x [admin_listinputqueue jobSchedule]
	upvar $x y
	return [lsort -integer [array names y]]
    } elseif { $element == "JOB"  && $elementArg != "" } {
	set joblist [admin_listinputqueue jobSchedule]
	upvar $joblist theArray
	set JobId $elementArg;
        if { [catch {set test [lindex $theArray($JobId) 1]}] } {
	    puts "Job with ID does not exist."
	} else {
	    puts "Job ID:           $JobId";
	    puts "Job Owner:        [lindex $theArray($JobId) 0]";
	    puts "OS Credentials:   [lindex $theArray($JobId) 1]";
	    puts "Target Object:    [lindex $theArray($JobId) 2]";
            puts "Target Type:      [lindex $theArray($JobId) 3]";
	    puts "Schedule:         [lindex $theArray($JobId) 6]";
	    puts "Status Flags:     [lindex $theArray($JobId) 7], [lindex $theArray($JobId) 8], [lindex $theArray($JobId) 9]";
	}
    } elseif { $element == "EVENTS"} {
	set x [admin_listinputqueue eventRegistrations]
	upvar $x y
	return [lsort -integer [array names y]]
   } elseif { $element == "EVENT" && $elementArg != "" } {
	set eventlist [admin_listinputqueue eventRegistrations]
	upvar $eventlist theArray;
	set EventId $elementArg;
        if { [catch {set test [lindex $theArray($EventId) 1]}] } {
	    puts "Event with ID $EventId does not exist."
	} else {
	    puts "Event ID:         $EventId";
	    puts "Event Name:       [lindex $theArray($EventId) 0]";
	    puts "Event Owner:      [lindex $theArray($EventId) 2]";
	    puts "Target Object:    [lindex $theArray($EventId) 1]";
	    puts "Polling Interval: [lindex $theArray($EventId) 4] seconds";
	    puts "Event Flags:      [lindex $theArray($EventId) 5]";
	    puts "Fixit Job:        [lindex $theArray($EventId) 3]";
	}
    } elseif { $element == "BLACKOUTS"} {
	set x [admin_listinputqueue blackoutRegistrations]
	upvar $x y
	return [lsort -integer [array names y]]
    } elseif { $element == "BLACKOUT"  && $elementArg != "" } {
	set blackoutlist [admin_listinputqueue blackoutRegistrations]
	upvar $blackoutlist theArray
	set blackoutId $elementArg;
        if { [catch {set test [lindex $theArray($blackoutId) 1]}] } {
	    puts "Blackout with ID does not exist."
	} else {
	    puts "Blackout ID:      $blackoutId";
	    puts "Blackout Owner:   [lindex $theArray($blackoutId) 0]";
	    puts "OS user:          [lindex $theArray($blackoutId) 1]";
	    puts "Target Name:      [lindex $theArray($blackoutId) 2]";
	    puts "Target Type:      [lindex $theArray($blackoutId) 3]";
	    puts "Blackout Name:    [lindex $theArray($blackoutId) 4]";
	    puts "Blackout Desc:    [lindex $theArray($blackoutId) 5]";
	    puts "Schedule:         [lindex $theArray($blackoutId) 6]";
	    puts "Subsystems:       [lindex $theArray($blackoutId) 7]";
	}
    } elseif { $element == "JOB_NOTIFICATIONS" && $elementArg != "" } {
        set userlist [admin_listinputqueue users]
	upvar $userlist theArray
	set Name $elementArg;
        if { [catch {set test [lindex $theArray($Name) 1]}] } {
	    puts "Uknown user: $Name"
	} else {
	    set x [admin_listoutputqueue jobStatus $Name ]
	    upvar $x y
	    return [lsort -integer [array names y]]
	}
    } elseif { $element == "JOB_NOTIFICATION" && $elementArg != "" && $elementArg2 != ""} {
        set userlist [admin_listinputqueue users]
	upvar $userlist theArray
	set Name $elementArg;
        if { [catch {set test [lindex $theArray($Name) 1]}] } {
	    puts "Uknown user: $Name"
	} else {
	    set x [admin_listoutputqueue jobStatus $Name ]
	    upvar $x y
	    set id $elementArg2
	    if { [catch {set test [lindex $y($id) 1]}] } {
		puts "Job Notification with ID $id does not exist."
	    } else {
		puts "Notification ID:  $id"
		puts "Job ID:           [lindex $y($id) 0]"
		puts "Target Object:    [lindex $y($id) 1]";
		puts "Object Type:      [lindex $y($id) 2]";
		puts "Generation Time:  [lindex $y($id) 3]";
                puts "Job Status:       [lindex $y($id) 4]";
		puts "Output:           [lindex $y($id) 6]";
		puts "Journal File:     [lindex $y($id) 7]";
	    }
	}
    } elseif { $element == "EVENT_REPORTS" && $elementArg != "" } {
        set userlist [admin_listinputqueue users]
	upvar $userlist theArray
	set Name $elementArg;
        if { [catch {set test [lindex $theArray($Name) 1]}] } {
	    puts "Uknown user: $Name"
	} else {
	    set x [admin_listoutputqueue eventReports $Name]
	    upvar $x y
	    return [lsort -integer [array names y]]
	}
    } elseif { $element == "EVENT_REPORT" && $elementArg != "" && $elementArg2 != ""} {
        set userlist [admin_listinputqueue users]
	upvar $userlist theArray
	set Name $elementArg;
        if { [catch {set test [lindex $theArray($Name) 1]}] } {
	    puts "Uknown user: $Name"
	} else {
	    set x [admin_listoutputqueue eventReports $Name ]
	    upvar $x y
	    set id $elementArg2
	    if { [catch {set test [lindex $y($id) 1]}] } {
		puts "Event Report with ID $id does not exist."
	    } else {
		puts "Notification ID:  $id"
		puts "Evant Name:       [lindex $y($id) 0]"
		puts "Target Object:    [lindex $y($id) 3]";
		puts "Generation Time:  [lindex $y($id) 2]";
		puts "Type:             [lindex $y($id) 1]";
		puts "Output:           [lindex $y($id) 5]";
	    }
	}
    }  else {
	error $usage;
    }
}

# Noop proc : can be overridden in event scripts for registration time checks
proc ValidateEvent {} {
  return 0
}

proc nls_regsub args {
    return [eval regsub $args]
}


proc strappend args {
    set nargs [llength $args]
    if { $nargs < 2 } {
        error "Usage : strappend <varname> arg1 \[arg2 ...\]"
    }
    set varname [lindex $args 0]
    upvar $varname strvar
    for { set x 1 } { $x < $nargs } { incr x } {
        # If string is non-empty and does not end with a space, append one
        if { [string length $strvar] != 0 && \
              ! [string match "* " $strvar] } {
            append strvar " "
        }
        append strvar [lindex $args $x]
    }
    return $strvar
}

proc escape_special_characters { original_string } {
    set ret_string $original_string

    # escape "\"
    set ret_string [escape_special_char $ret_string "\\"]

    # escape "
    set ret_string [escape_special_char $ret_string "\""]

    # escape [
    set ret_string [escape_special_char $ret_string "\["]

    # escape ]
    set ret_string [escape_special_char $ret_string "\]"]

    # escape {
    set ret_string [escape_special_char $ret_string "\{"]

    # escape }
    set ret_string [escape_special_char $ret_string "\}"]

    # escape $
    set ret_string [escape_special_char $ret_string "\$"]

    return $ret_string
}

proc escape_special_char { original_string special_char } {
    set ret_string $original_string

    set location [string first "$special_char" $ret_string]
    while { $location != -1 } {
        set ret_string [string replace $ret_string $location $location "\\$special_char"]
        set location [string first "$special_char" $ret_string [expr $location + 2]]
    }

    return $ret_string
}


###############################################
#
# see bug 1375632
#
# Usage of FailureCheck
#    (1) declare an oraeventpersist variable last_err_msg
#        oraeventpersist last_err_msg {}
#    (2) in proc EvalEvent, declare globals
#        global last_report output last_err_msg
#    (3) whenever an error catched, save the error message in output, then
#        return [ FailureCheck last_report last_err_msg $output ]
#
###############################################

proc FailureCheck { last_report_ref last_err_msg_ref curr_err_msg } {

    global SCRIPT_FAIL NO_EVENT
    upvar $last_report_ref last_report
    upvar $last_err_msg_ref last_err_msg

    if { $last_report == $SCRIPT_FAIL && $last_err_msg == $curr_err_msg } {
        return $NO_EVENT
    } else {
        set last_report $SCRIPT_FAIL
        set last_err_msg $curr_err_msg
        return $SCRIPT_FAIL
    }
}


#########################################################
#
# get ORACEL_HOME for an OPS database & instances
# use target_name to get unique entry out of snmp_ro.ora
#
# Usage to check failure cases
#   if { [catch {set oracle_home [get_ops_home $oramsg(oraobject)]} err} {
#       handle error (error message stored in $err)
#   }
#
#########################################################

proc get_ops_home { target_name } {

    set TargetOH ""
    set error_msg ""
    set FoundTarget 0
    set orafile [concatname [list [AGENT_ORACLE_HOME] network admin snmp_ro.ora]]

    if {[file exists $orafile]} {
        if {[catch {set fd [open $orafile r]} err]} {
            # error opening snmp_ro.ora
            ORATCL_DEBUG "$target_name : [oratime] : Error opening $orafile, error = $err"
            lappend error_msg [format [msgtxt [NETWORK] nms 312] snmp_ro.ora]
            return -code 1 $error_msg
        } else {
            # success, snmp_ro.ora is open
            # look for entry with matching target_name
            append searchStr "snmp.oraclehome." [string trim $target_name]

            while {[gets $fd line] >= 0} {
                ORATCL_DEBUG "$target_name : [oratime] : Found snmp_ro.ora line = $line"

                set tmpLine [split $line =]
                set leftPart [string trim [lindex $tmpLine 0]]
                set rightPart [string trim [lindex $tmpLine 1]]

                if { [string compare $leftPart $searchStr] == 0 } {
                    set FoundTarget 1
                    set TargetOH $rightPart
                    break
                }
            }
            close $fd
        }
    } else {
        # snmp_ro.ora does not exist: Can't happen unless it is removed after agent started.
        ORATCL_DEBUG "$target_name : [oratime] : fatal error: snmp_ro.ora does not exist."
        lappend error_msg "Fatal error: $orafile does not exist."
        return -code 1 $error_msg
    }

    if { $FoundTarget == 0 } {
        # failure: did not find TargetName, TargetType in snmp_ro.ora
        ORATCL_DEBUG "$target_name : [oratime] : fatal error: snmp_ro.ora does not contain this target."
        lappend error_msg "The Agent does not have this service $target_name in it's snmp_ro.ora"
        return -code 1 $error_msg
    } else {
        return -code 0 $TargetOH
    }

}

# Registration validation for Data Guard Manager Events
proc ValidateDGMEvent {connect_str} {

    # Declare globals we will use
    global output
    global SCRIPT_FAIL NO_EVENT
    global oramsg test_name

    ORATCL_DEBUG "$test_name : $oramsg(oraobject) : [oratime] : ValidateDGMEvent"

    # initialize the return code and output
    set output ""
    set err ""
    set db_version ""

    # Connect to the database
    set connect [format "%s@%s" [convertin $oramsg(agent_characterset) $connect_str] $oramsg(oraobject)]
    if {[catch {oralogon_uncached $connect "SYSDBA"} lda]} {
        lappend output [msgtxt [RDBMS] ora $oramsg(rc)] ""
        return $SCRIPT_FAIL
    }

    # Open a database cursor.
    if {[catch {set dbcur [oraopen $lda]} err]} {
        lappend output [convertout $oramsg(agent_characterset) $err]
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    # Get the database version
    set sql {select banner from v$version}
    ORATCL_DEBUG "$test_name : $oramsg(oraobject) : [oratime] : sql=$sql"
    if {[catch {orasql $dbcur $sql}]} {
        lappend output [convertout $oramsg(agent_characterset) \
            $oramsg(errortxt)] $sql
        catch {oraclose $dbcur} err
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    if {[catch {set row [orafetch $dbcur]} err]} {
        lappend output [convertout $oramsg(agent_characterset) $err]
        catch {oraclose $dbcur} err
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    if {$oramsg(rows) == 0} {
        lappend output [msgtxt [NETWORK] nms 1015] $sql
        catch {oraclose $dbcur} err
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    # Save the banner
    set banner [lindex $row 0]

    # Parse out the version number
    set period_index [string first "." $banner]
    # Concatenate the characters before and after the first period.
    set db_version [string index $banner [expr $period_index-1]]
    append db_version [string index $banner [expr $period_index+1]]
    ORATCL_DEBUG "$test_name : $oramsg(oraobject) : [oratime] : db_version from banner=$db_version"

    # Database version must be 9.2 or higher
    if {[string compare $db_version "92"] < 0} {
        lappend output [format [msgtxt [NETWORK] nms 1031] 9.2]
        catch {oraclose $dbcur} err
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    # The database must be part of a Configuration
    set sql {select value from v$parameter where name='dg_broker_start'}
    ORATCL_DEBUG "$test_name : $oramsg(oraobject) : [oratime] : sql=$sql"
    if {[catch {orasql $dbcur $sql}]} {
        lappend output [convertout $oramsg(agent_characterset) \
            $oramsg(errortxt)] $sql
        catch {oraclose $dbcur} err
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    if {[catch {set row [orafetch $dbcur]} err]} {
        lappend output [convertout $oramsg(agent_characterset) $err]
        catch {oraclose $dbcur} err
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    if {$oramsg(rows) == 0} {
        lappend output [msgtxt [NETWORK] nms 1015] $sql
        catch {oraclose $dbcur} err
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    # Save the drs_start
    set drs_start [lindex $row 0]
    ORATCL_DEBUG "$test_name : $oramsg(oraobject) : [oratime] : dg_broker_start=$drs_start"

    # drs_start must be TRUE
    if {[string compare [string toupper $drs_start] "TRUE"] != 0} {
        lappend output [msgtxt [NETWORK] nms 1032]
        catch {oraclose $dbcur} err
        catch {oralogoff_uncached $lda} err
        return $SCRIPT_FAIL
    }

    # Log off
    catch {oraclose $dbcur} err
    catch {oralogoff_uncached $lda} err

    # Success
    return $NO_EVENT
}

