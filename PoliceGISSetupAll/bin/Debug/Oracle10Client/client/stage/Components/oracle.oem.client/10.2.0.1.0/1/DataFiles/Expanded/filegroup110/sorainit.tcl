# Copyright (c) 1995, 2003, Oracle Corporation.  All rights reserved.  
#
# NAME:
#
#  sorainit.tcl : This file contains the OSD definitions and procedures
#                 used by all tcl scripts.
# $Log:  $
# Revision 1.5  04/03/2000  kduvvuri sgi merge - bug1243405
# Revision 1.4  1996/08/22  22:08:04  yliu
# added tempfile, mvfile, etc
#
# Revision 1.3  1996/03/07  22:29:17  ebosco
# Added import export load
#
# Revision 1.2  1995/11/08  19:22:12  yliu
# change df option for Solaris
#
# Revision 1.1  1995/08/11  01:25:07  yliu
# Initial revision
#

########################################################################
#
# Porting Note : 
# Most of the procedures in this file are based on Unix commands.
# If you need to change any of the procedures for you port, please do
# the following :
# if {$tcl_platform(platform) == "your platform name"} {
#      do your platform stuff
# } else {
#      the original code
# }
#
# You can use nmitclsh or tclsh to acquire your platform name. Use the 
# following command in nmitclsh or tclsh :
# puts $tcl_platform(os) 
#
########################################################################

proc concatname {components} {
    # Given file name components, construct a complete file name
    # For VMS, only the last component is returned, because symbols
    # are used to invoke programs on VMS, not full paths.
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        set full_path [lindex $components [expr [llength $components] -1]]
    } else {
        append full_path [lindex $components 0]
        set no_components [llength $components]
        set i 1
        while {$i < $no_components} {
            append full_path "/"
            append full_path [lindex $components $i]
            incr i
        }
   }
    return $full_path
}


proc AGENT_ORACLE_HOME {} {
    # return the oracle_home of the agent installation
    global env
    return $env(ORACLE_HOME)
}


proc ORACLE_HOME {} {
    # return the oracle of the target service
    global oramsg
    set oHome $oramsg(orahome)
    return $oHome
}


proc ORACLE_SID {} {
    # return the oracle_sid of the target database 
    global oramsg
    set temp_list [oradbsnmp get applName.$oramsg(oraindex)]
    set osid [lindex $temp_list 1]
    return $osid
}

proc DB_VERSION {} {
    # return the Database Version of the target database
    # Will return NULL string if the database version is not available,
    # the version will not be available if the database or listener is down.
    global oramsg
    set dbver " "
    if { ![catch {oradbsnmp get applVersion.$oramsg(oraindex)} out] } {
    	if {[string match "*Server not open*" $out] == 0 } {
    		set ver [lindex $out 1]
    		regsub -all {\.} $ver {} ver
    		set dbver [string range $ver 0 1]
    		return $dbver
	}
    }
    return $dbver
}
    
proc DEFAULT_INITFILE {} {
    # return the default filename (full path) of "init.ora"
    global oramsg
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        set fn "ORA_PARAMS"
    } else {
        append fn [ORACLE_HOME] "/dbs/" "init" [ORACLE_SID] ".ora"
    }
    return $fn
}    


proc LSNRCTL { { Listener "" } } {
    # return the full path name for listener control application
    # Unix, return symbol invoking program for VMS
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        set lsnrctlexec "LSNRCTL";
    } else {
        set lsnrctlexec ""
        if { $Listener == "" } {
           if {[file exists [concatname [list [AGENT_ORACLE_HOME] bin lsnrctl]]]} {
              set lsnrctlexec [concatname [list [AGENT_ORACLE_HOME] bin lsnrctl]]
           }
        } else {
           if {[file exists [concatname [list [lindex [getlsnrdata $Listener] 3] bin lsnrctl]]]} {
	      set lsnrctlexec [concatname [list [lindex [getlsnrdata $Listener] 3] bin lsnrctl]]
           }
        }
    }
    return $lsnrctlexec
}

proc getlsnrdata { name } {
    # returns the listener entries from snmp_ro.ora 
    set shortname ""
    set longname ""
    set configfile ""
    set homename ""
 
    set snmprohdl [open [SNMPRO_FILE] r]
    while {[gets $snmprohdl line] >= 0} {
       if {[regexp -nocase "snmp.shortname.$name *= " $line]} {
           set shortname [string trim [string range $line [expr [string first = $line] +1] end] " "]
       }
       if {[regexp -nocase "snmp.longname.$name * =" $line]} {
           set longname [string trim [string range $line [expr [string first = $line] +1] end] " "]
       }
       if {[regexp -nocase "snmp.configfile.$name *=" $line]} {
           set configfile [string trim [string range $line [expr [string first = $line] +1] end] " "]
       }
       if {[regexp -nocase "snmp.oraclehome.$name *=" $line]} {
           set homename [string trim [string range $line [expr [string first = $line] +1] end] " "]
       }
    }
    close $snmprohdl
    return [list $shortname $longname $configfile $homename]
}

proc SQLDBA {} {
    # return the full path name of the sqldba executable
    set sqldbaexec "" ;
    if {[file exists [concatname [list [ORACLE_HOME] bin sqldba]]]  } {
	set sqldbaexec [concatname [list [ORACLE_HOME] bin sqldba]];
    }
    return $sqldbaexec
}


proc SQLPLUS {} {
    # return the full path name of the sqlplus executable
    # return the symbol invoking sqlplus for VMS
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        set sqlplusexec "SQLPLUS";
    } else {
        set sqlplusexec "";
        if {[file exists [concatname [list [ORACLE_HOME] bin sqlplus]]]  } {
	      set sqlplusexec [concatname [list [ORACLE_HOME] bin sqlplus]];
        }
    }
    return $sqlplusexec
}


proc SVRMGRL {} {
    # return the full path name of the svrmgrl executable
    # symbol that runs it if VMS
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        set svrmgrlexec "SVRMGRL"
    } else {
        set svrmgrlexec "";
        if {[file exists [concatname [list [ORACLE_HOME] bin svrmgrl]]]  } {
           set svrmgrlexec [concatname [list [ORACLE_HOME] bin svrmgrl]];
        }
    } 
    return $svrmgrlexec
}


proc RMAN {} {
    # return the full path name of the rman executable
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        set rmanexec "RMAN"
    } else {
        set rmanexec "";
        if {[file exists [concatname [list [ORACLE_HOME] bin rman]]]  } {
           set rmanexec [concatname [list [ORACLE_HOME] bin rman]];
        }
    } 
    return $rmanexec
}   


proc OS_SHELL_EXEC {command} {
    # execute a command within an OS shell
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
       exec $command
    } else {
       set tmpfile [tempfile osc]
       set code [catch {exec sh -c $command >& $tmpfile}]
       set fd [open $tmpfile r]
       set output [read $fd]
       close $fd
       rmfile $tmpfile
       return -code $code $output
    }
}


proc BROADCAST {message} {
    # broadcast the message
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        exec reply \"$message\"
    } else {
        set tmpfile [tempfile bdc]
        set fd [open $tmpfile w]
        puts $fd $message
        close $fd
       if {$tcl_platform(os) == "DYNIX/ptx"} {
           # for sequent
            exec /etc/wall < $tmpfile
       } elseif {$tcl_platform(os) == "AIX" || $tcl_platform(os) == "HP-UX" || $tcl_platform(os) == "OSF1" || $tcl_platform(os) == "UnixWare" || $tcl_platform(os) == "dgux" } { 
            exec /usr/sbin/wall < $tmpfile
       } elseif {$tcl_platform(os) == "Linux"} {
            exec /usr/bin/wall < $tmpfile
       } elseif {$tcl_platform(os) == "IRIX64"} {
            exec /etc/wall < $tmpfile
       } else {
            exec /usr/sbin/wall -a < $tmpfile
       }
       rmfile $tmpfile
    }
}


proc TNS_ADMIN {} {
    # return the tns_admin directory of the agent's environment
    global env
     global tcl_platform
    if {[info exists env(TNS_ADMIN)] && [file isdirectory $env(TNS_ADMIN)]} {
	set dir $env(TNS_ADMIN)
    } else {
        if {$tcl_platform(os) == "VMS"} {
            set dir "TNS_ADMIN:"
        } else {
	    set dir [AGENT_ORACLE_HOME]/network/admin
        }
    }
    return $dir
}


proc RDBMS {} {
    # return the name of the rdbms product
    return "rdbms"
}


proc NETWORK {} {
    # return the name of the network product
    return "network"
}


proc NETWORK_TRACE_DIR {} {
    # return default directory where the trace files go
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        append dir {ORA_ROOT:[} [NETWORK] {.TRACE]}
    } else {
        append dir [AGENT_ORACLE_HOME] "/" [NETWORK] "/" trace
    }
    return $dir
}


proc SNMPRO_FILE {} {
    # return the fully qualified  filename for snmp_ro.ora 
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        append fn [TNS_ADMIN] snmp_ro.ora
    } else {
        append fn [TNS_ADMIN] "/" snmp_ro.ora
    }
    if { [file exists $fn] } {
	return $fn
    }
}


proc SNMPRW_FILE {} {
    # return the fully qualified  filename for snmp_rw.ora 
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        append fn [TNS_ADMIN] snmp_rw.ora
    } else {
        append fn [TNS_ADMIN] "/" snmp_rw.ora
    }
    if { [file exists $fn] } {
	return $fn
    }
}


proc SERVICES_FILE {} {
    # return the fully qualified  filename for services.ora 
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        set fn "ORA_AGENT:SERVICES.ORA"
    } else {
        append fn [AGENT_ORACLE_HOME] "/" [NETWORK] "/" agent "/" services.ora
    }
    if { [file exists $fn] } {
	return $fn
    }
}


proc DBSNMPVER_FILE {} {
    # return the fully qualified  filename for services.ora 
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        set fn "ORA_AGENT:DBSNMP.VER"
    } else {
        append fn [AGENT_ORACLE_HOME] "/" [NETWORK] "/" agent "/" dbsnmp.ver
    }
    if { [file exists $fn] } {
	return $fn
    }
}

proc diskstats {file_list} {
    # Input: a list of the files or empty for all file systems
    # Output: 5 lists, filesystems, total space, avail space, pct free and 
    #         mount points
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
      if {[llength $file_list] == 0} {
        ORATCL_DEBUG "diskstats: error: file_list is empty"
        error ""
      }
      set no_file [llength $file_list]
      set i 0
      while { $i < $no_file } {
        set dev_name [lindex $file_list $i]
        lappend filesystem $i
        if { [catch {exec @ora_agent:disk_usage $dev_name {"TOTAL"}} s_total] ||
             [catch {exec @ora_agent:disk_usage $dev_name {"AVAIL"}} s_free] } {
          ORATCL_DEBUG "diskstats: \@ora_agent:disk_usage failed: $temp"
          error ""
        }
        lappend total [expr $s_total + 0]
        lappend free [expr $s_free + 0]
        lappend pct_free [ expr round(($s_free + 0.0) * 100  / $s_total) ]
        lappend mount $dev_name
        incr i
      }
    } else {
      # various UNIX flavours 

      #  default settings - suits most Unix platforms
      if {[llength $file_list] > 0} {
        set df_cmd "df -k"
      } else {
        set df_cmd "df -k -l"
      }
      # relative position within one line
      set df_cols 6
      set filesystem_col 0
      set total_col 1
      set free_col 3
      set pct_col 4
      set mount_col 5

      # exceptions: either different command, switches or layout of output
      if {$tcl_platform(os) ==  "HP-UX" } {
        if {[llength $file_list] > 0} {
          set df_cmd "df -P -k"
        } else {
          set df_cmd "df -P -k -l"
        }
      } elseif {$tcl_platform(os) ==  "AIX" } {
        set df_cmd "df -k"
        set df_cols 7
        set free_col 2
        set pct_col 3
        set mount_col 6
      } elseif {$tcl_platform(os) ==  "IRIX64" } {
        set df_cols 7
        set filesystem_col 0
        set total_col 2
        set free_col 4
        set pct_col 5
        set mount_col 6
      } 

      if {[llength $file_list] > 0} {
        set df_cmd [ concat $df_cmd $file_list ]
      }

      # issue command to determine the disk usage
      # using a pipe instead of exec to get rid of the unwanted header
      if { [ catch { open "|$df_cmd"} cmd_pipe] } {
        ORATCL_DEBUG "diskstats: open failed: $cmd_pipe"
        error ""
      }

      set i 0
      set no_file 0
      while {[gets $cmd_pipe line] >= 0} {
        if { ($i > 0) && ([lindex $line $total_col] != 0) } {
           lappend filesystem [lindex $line $filesystem_col]
           lappend total [lindex $line $total_col]
           lappend free [lindex $line $free_col]
           set used [lindex $line $pct_col]
           regexp {([0-9]+)} $used junk capacity
           lappend pct_free [expr 100 - $capacity]
           lappend mount [lindex $line $mount_col]
           incr no_file
        }
        incr i
      }
      # any errors during command execution will be reported on close of pipe
      if { [ catch { close $cmd_pipe } cmd_err] } {
        ORATCL_DEBUG "diskstats: close failed: $cmd_err"
        error ""
      }
    }
    if { $no_file > 0 } {
      lappend ret_val $filesystem $total $free $pct_free $mount
    } else {
      ORATCL_DEBUG "diskstats: error: no valid filesystem"
      error ""
    }
    return $ret_val
}


proc diskusage {file_list} {
    # Input: a list of the files or empty for all file systems
    # Output: 4 lists, filesystems, total space, percent of avail space and mount points

    if { [catch { diskstats $file_list} temp] } { 
      error "" 
    } 
  
    lappend ret_val [lindex $temp 0] [lindex $temp 1] [lindex $temp 2] [lindex $temp 4] 

    return $ret_val
}

proc pctdiskusage {file_list} {
    # Input: a list of the files or empty for all file systems
    # Output: 4 lists, filesystems, total space, percent of avail space and mount points

    if { [catch { diskstats $file_list} temp] } { 
      error "" 
    } 
  
    lappend ret_val [lindex $temp 0] [lindex $temp 1] [lindex $temp 3] [lindex $temp 4] 
    return $ret_val
}

proc swapusage {} {
    # returns the percent of available swap space 
    # for solaris:
    global tcl_platform
  
    if {$tcl_platform(os) == "AIX"} {
       catch {exec /usr/sbin/lsps -s} buffer
       regexp {([0-9]+)%} $buffer whole used_pect
       set avail_pct [expr 100 - $used_pect]
       return $avail_pct
    } elseif {$tcl_platform(os) == "OSF1"} {
       # for compaq
       catch {exec /sbin/swapon -s} buffer
       set finish [string length $buffer]
       set fin1 [expr $finish -98]
       set st1 [expr $finish -101]
       set total [string range $buffer $st1 $fin1]
       set tkbytes [expr $total * 10]
       set pfin1 [expr $finish -3]
       set pst1 [expr $finish -5]
       set percent [string range $buffer $pst1 $pfin1]
       set avail [expr $tkbytes * $percent]
       return $percent
    } elseif {$tcl_platform(os) == "DYNIX/ptx"} {
       # for sequent:
       catch {exec /etc/swap -f} buffer
       regexp {([0-9]+) free} $buffer whole avail
       return [expr $avail/2]
    } elseif {$tcl_platform(os) == "HP-UX"} {
       catch {exec /usr/sbin/swapinfo -t} buffer
       set used [lindex $buffer [expr [llength $buffer] - 4]]
       regexp {([0-9]+)%} $used whole used_pct
       set pct_free [expr 100 - $used_pct]
       return $pct_free
    } elseif {$tcl_platform(os) == "Linux"} {
       catch {exec /usr/bin/free -o -m | grep Swap} buffer
       scan $buffer "%s%s%s%s" junk1 total used free
       set free_dbl [format %3.2f $free]
       set total_dbl [format %3.2f $total]
       set pct_free [expr [expr $free_dbl / $total_dbl] * 100]
       return $pct_free
    } elseif {$tcl_platform(os) == "UnixWare"} {
       catch {exec /etc/swap -s} buffer
       regexp {([0-9]+) blocks available} $buffer whole avail
       regexp {([0-9]+) blocks used} $buffer used_whole used
       set avail_dbl [format %3.2f $avail]
       set used_dbl [format %3.2f $used]

       set pct_avail [expr [expr $avail_dbl / [expr $used_dbl + $avail_dbl]] * 100]
       return $pct_avail
   
    } elseif {$tcl_platform(os) == "dgux"} {
      #for DG
      catch {exec nsar -r 5 2 | grep Average} buffer
      scan $buffer "%s%s%s" ave freemem freeswp
      set avail [expr [expr $freeswp - $freemem] / 2 ]
      exec echo "{s += \$2; print s}" > /tmp/pat
      catch {exec admswap -o list -bu | grep y | nawk -f /tmp/pat | tail -1} total 
      exec /bin/rm /tmp/pat
      set avail_pct [expr [expr $avail / $total] * 100 ] 
      return $avail_pct
    } elseif {$tcl_platform(os) == "IRIX64"} {
      catch {exec /etc/swap -sb} buffer

      regexp {([0-9]+) blocks available} $buffer whole avail_tmp
      set avail [expr $avail_tmp / 2]

      regexp {([0-9]+) blocks used} $buffer used_whole used_tmp
      set used [expr $used_tmp / 2]

      set avail_dbl [format %3.2f $avail]
      set used_dbl [format %3.2f $used]

      set pct_avail [expr [expr $avail_dbl / [expr $used_dbl + $avail_dbl]] * 100]
      return $pct_avail
    } elseif { $tcl_platform(os) == "VMS" } {
      catch {exec @ora_agent:avail_swap_pages} avail
    } else {

      catch {exec /etc/swap -s} buffer

      set tmp1 [split $buffer =]
      set tmp2 [split [lindex $tmp1 1] ,]

      set used  [string trim [lindex [lindex $tmp2 0] 0] " k"]
      set avail [string trim [lindex [lindex $tmp2 1] 0] " k"]

      set avail_dbl [format %3.2f $avail]
      set used_dbl [format %3.2f $used]

      set pct_avail [expr [expr $avail_dbl / [expr $used_dbl + $avail_dbl]] * 100]

      return $pct_avail
    }
}


proc getcpuutil {} {
    # returns the cpu utilization (user+system) on the node
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        catch {exec @ora_agent:cpu_utilization} cpu_pct
        return $cpu_pct
    } elseif {$tcl_platform(os) == "AIX"} { 
        catch {exec vmstat 5 2 | tail -1} buffer
        scan $buffer "%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s" r b avm fre re pi po fr sr cy in sy cs user sys id wa
        return [expr $user + $sys]
    } elseif {$tcl_platform(os) == "DYNIX/ptx"} {
      #for sequent
       catch {exec sar 5 2} buffer
    } elseif {$tcl_platform(os) == "Linux"} {
       catch { exec vmstat -n 5 2 } buffer
       set total [string length $buffer]
       set idlestart [string wordstart $buffer [expr $total -1]]
       set idle [string range $buffer $idlestart end]
       return [expr 100 - $idle]
    } elseif {$tcl_platform(os) == "OSF1"} {
       catch {exec vmstat 5 2} buffer
       set finish [string length $buffer]
       set start [expr $finish - 9]
       set buffer1 [string range $buffer $start $finish]
       scan $buffer1 "%3s%3s%3s" user sys idle
       return [expr $user + $sys]
    } elseif {$tcl_platform(os) == "UnixWare"} {
       catch {exec sar -P ALL -u 2 | tail -2 | head -1} buffer
       scan $buffer "%s%s%s" name user sys 
       return [expr $user + $sys]
    } elseif  {$tcl_platform(os) == "dgux"} {
       #for DG
       catch {exec nsar -u 5 2 | grep Average } buffer
       scan $buffer "%s%s%s%s" ave  usr sys idle 
       return [expr $usr + $sys]
    } elseif  {$tcl_platform(os) == "IRIX64"} {
       #for IRIX 64bit
       catch {exec sar 5 2 | grep Average } buffer
       scan $buffer "%s%s%s%s" ave  usr sys idle
       return [expr $usr + $sys]
    } else {
      catch {exec vmstat 5 2} buffer
      set buffer [string trim $buffer]
      if {[string first "State change" $buffer] != -1} {
        set buffer ""
        catch {exec vmstat 5 2} buffer
        ORATCL_DEBUG "buffer =  $buffer"
          if {[string first "State change" $buffer] != -1} {
            error "Unexpected system change while evaluating cpuutil-retry failed"
          }
      }
    }
    if {$tcl_platform(os) == "HP-UX"} {
        set finish [string length $buffer]
        set start [expr $finish - 9]
        set buffer1 [string range $buffer $start $finish]
        scan $buffer1 "%3s%3s%3s" user sys idle
        return [expr $user + $sys]
    } else {
    # The parsing for Solaris works fine for Sequent as well since sar also
    # gives the idle time as the last field.
    # Removed Sequent specific code. See bug 1398809 for details
       set total [string length $buffer] 
       set idlestart [string wordstart $buffer [expr $total -1]]
       set idle [string range $buffer $idlestart end]
       return [expr 100 - $idle]
    }
}


proc getpaging {} {
    # returns the paging activity on the node in KB per sec
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        catch {exec @ora_agent:page_fault_rate} pf_rate
        return $pf_rate
    } elseif {$tcl_platform(os) == "AIX"} {
        catch {exec vmstat 5 2} buffer
        set finish [expr [string length $buffer] - 26]
        set start [expr $finish - 30]
        set buffer1 [string range $buffer $start $finish]
        scan $buffer1 "%5s%4s%4s%4s%4s%5s%4s" fre re pi po fr sr cy
        return [divide [expr $pi + $po] 5]
    } elseif {$tcl_platform(os) == "HP-UX"} {
       catch {exec vmstat 5 2} buffer
       set finish [string length $buffer]
       set finish [expr $finish -23]
       set start [expr $finish - 45]
       set buffer1 [string range $buffer $start $finish]
       scan $buffer1 "%5s%5s%6s%5s%6s%5s%6s%7s" re at pi po fr de sr in
       return [divide [expr $pi + $po] 5]
    } elseif {$tcl_platform(os) == "OSF1"} {
       # for compaq
       catch {exec vmstat 5 2} buffer
       set finish [expr [string length $buffer] - 21]
       set start [expr $finish -10]
       set buffer1 [string range $buffer $start $finish]
       scan $buffer1 "%5s%5s" pi po
       return [divide [expr $pi + $po] 5]
    } elseif {$tcl_platform(os) == "DYNIX/ptx"} {
       # for sequent
       catch {exec sar -w 5 2} buffer
       set finish [string length $buffer]
       set start [expr $finish - 39]
       set buffer1 [string range $buffer $start $finish]
       scan $buffer1 "%5s%5s%5s" pi bi po
       return [divide [expr $pi + $po] 5]
    } elseif {$tcl_platform(os) == "UnixWare"} {
       catch {exec sar -p | grep Av | tail -1} bufpgi
       catch {exec sar -g | grep Av | tail -1} bufpgo
       scan $bufpgi "%s%s%s%s%s%s" name at atf atm pi rest
       scan $bufpgo "%s%s%s%s%s%s" name at atf atm po rest 
       set pi [expr $pi * 100]
       set po [expr $po * 100] 
       return [expr $pi + $po] 
    } elseif {$tcl_platform(os) == "dgux"} {
      #for DG
      catch {exec nsar -p 5 2 | grep Average } buffer
      scan $buffer "%s%s%s%s%s%s" ave atch pgin ppgin pflt vflt
      catch {exec nsar -g 5 2 | grep Average } buffer
      scan $buffer "%s%s%s%s%s" ave pgout ppgout pgfree pgscan
      return [expr $pgin + $pgout] 
   } elseif  {$tcl_platform(os) == "IRIX64"} {
       #for IRIX 64bit
       catch {exec sar -w 5 2 | grep Average } buffer
       scan $buffer "%s%s%s%s%s" ave  pi bpi po extra
       return [divide [expr $pi + $po] 5]
   } elseif  {$tcl_platform(os) == "Linux"} {
      #Collect paging stat - run 1
      catch {exec cat /proc/stat | grep page} page_run1
      catch {exec date +%s} sec_run1
      ## make the program sleep for 1 second
      catch {exec sleep 1} junk
      #Collect paging stat - run 2
      catch {exec cat /proc/stat | grep page} page_run2
      catch {exec date +%s} sec_run2
      #debug statements
      ORATCL_DEBUG "run1 =  $page_run1"
      ORATCL_DEBUG "run2 =  $page_run2"
      ORATCL_DEBUG "secs =  $sec_run1, $sec_run2"
      #get actual data from tmp buffers
      ###paging - run 1
      scan $page_run1 "%s%s%s" page pi1 po1
      ORATCL_DEBUG "Actual pi1=$pi1, po1=$po1"
      ###paging - run 2
      scan $page_run2 "%s%s%s" page pi2 po2
      ORATCL_DEBUG "Actual pi2=$pi2, po2= $po2"
      #calculate  paging per second
      set time_diff [expr $sec_run2 - $sec_run1]
      if {$time_diff == 0} {
          set time_diff 1
      }
      set tot_pi [expr $pi2 - $pi1]
      set tot_po [expr $po2 - $po1]
      set tot_paging [expr $tot_pi + $tot_po]
      set output [divide $tot_paging $time_diff]
      ORATCL_DEBUG " Value : $output "
      return $output
    } else {
      catch {exec vmstat 5 2} buffer
      if {[string first "State change" $buffer] != -1} {
        set buffer ""
        catch {exec vmstat 5 2} buffer
          if {[string first "State change" $buffer] != -1} {
            error "Unexpected system change while evaluating paging - retry failed"
          }
      }
       set count 0
       while {$count < 15} {
         set buffer1 [string trim $buffer]
         set total [string length $buffer1]
         set indexoflastword [string wordstart $buffer1 [expr $total -1]]
         if {$count == 13} {
             set po [string range $buffer1 $indexoflastword end]
         }
         if {$count == 14} {
            set pi [string range $buffer1 $indexoflastword end]
            break
         }
         set buffer [string range $buffer1 0 [expr $indexoflastword -1]]
         set count [expr $count + 1]
       } 
       return [divide [expr $pi + $po] 5]
    }
}


proc tempdir {} {
    # returns a directory name which will be used to store temporary files
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        return "ORA_AGENT_TMP:"
    } else {
      return "/tmp"
    }
}


proc tempfile {ext} {
    # returns a temporary file name given its extention
    global tcl_platform
    set base0 [lindex [oratime] 3]
    regsub -all {:} $base0 {} base
    if {$tcl_platform(os) == "VMS"} {
        set temp [tempdir]
        append temp $base.$ext
    } else {
      set temp [concatname [list [tempdir] $base.$ext]]
    }
    set i 0
    while {[file exists $temp] == 1} {
       if {$tcl_platform(os) == "VMS"} {
            set temp [tempdir]
            append temp $base$i.$ext
        } else {
            set temp [concatname [list [tempdir] $base$i.$ext]]
        }
        incr i
    }
    return $temp
}

proc rmfile {filename} {
    # remove a file 
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        if {[string last ";" $filename] != [string length $filename]-1} {
            append filename ";"
        }
        exec delete/nolog $filename
    } else {
      exec /bin/rm -f $filename
    }
}

proc catfile {filename} {
    # display a file
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        exec type $filename
    } else {
      exec /bin/cat $filename
    }
}

proc echofile {input filename} {
    # echo a string to a file
    global tcl_platform
    if {$tcl_platform(os) == "VMS"} {
        exec type/output=$filename $input
    } else {
        exec /bin/echo $input > $filename
    }
}

proc mvfile {filenames destination} {
    # move files to other location
    eval exec /bin/mv [glob $filenames] $destination
}


proc import args {
    # execute import
    global oramsg
    global tcl_platform
    setOracleEnvironment 
    if {$tcl_platform(os) == "VMS"} {
        # The brackets have to be stripped from the args
        set args [string range $args [expr [string first "{" $args] + 1] [expr [string last "}" $args] - 1]]
        exec imp $args
    } else {
        set args [join $args " "]
        exec /bin/sh -c "$oramsg(orahome)/bin/imp $args < /dev/null"
    }
    restoreOracleEnvironment 
}
 

proc export args {
    # execute export
    global oramsg
    global tcl_platform
    setOracleEnvironment 
    if {$tcl_platform(os) == "VMS"} {
        # The brackets have to be stripped from the args
        set args [string range $args [expr [string first "{" $args] + 1] [expr [string last "}" $args] - 1]]
        exec exp $args
    } else {
        set args [join $args " "]
        exec /bin/sh -c "$oramsg(orahome)/bin/exp $args < /dev/null"
    }
    restoreOracleEnvironment 
}
 

proc loader args {
    # execute sql loader
    global oramsg
    global tcl_platform
    setOracleEnvironment
    if {$tcl_platform(os) == "VMS"} {
        # The brackets have to be stripped from the args
        set args [string range $args [expr [string first "{" $args] + 1] [expr [string last "}" $args] - 1]]
        exec sqlldr $args
    } else {
        set args [join $args " "]
        exec /bin/sh -c "$oramsg(orahome)/bin/sqlldr $args < /dev/null"
    }
    restoreOracleEnvironment 
}

proc ALERTFILE {} {
    # return the alert file name of the target database
    global oramsg 
    global tcl_platform
    if {[catch { set temp_list [oradbsnmp get rdbmsSrvParamCurrValue.$oramsg(oraindex).20.98.97.99.107.103.114.111.117.110.100.95.100.117.109.112.95.100.101.115.116.1] } err]} {
       ORATCL_DEBUG "ALERTFILE:: oradbsnmp failed : $err"
       return ""
    }
    if {$tcl_platform(os) == "VMS"} {
        regsub {[?]} [lindex $temp_list 1] $oramsg(orahome) dump_dest
        catch {exec @ora_agent:current_nodename} curr_node
        set curr_node [string range $curr_node 0 [expr [string length $curr_node]-2]]
        append alert_name $dump_dest ":" $curr_node "_" [lindex $temp_list 1] "_alert.log"
    } else {
        regsub {[?]} [lindex $temp_list 1] $oramsg(orahome) dump_dest
        regsub {[@]} $dump_dest [ORACLE_SID] dump_dest
        lappend components $dump_dest
        set temp_list [oradbsnmp get applName.$oramsg(oraindex)]
        append file_name "alert_" [lindex $temp_list 1] ".log"
        lappend components $file_name
        set alert_name [concatname $components]
    }
        return $alert_name
}


proc BACKGROUND_DUMP_DEST {} {
    # return the background_dump destination of the target database
    global oramsg
    set temp_list [oradbsnmp get rdbmsSrvParamCurrValue.$oramsg(oraindex).20.98.97.99.107.103.114.111.117.110.100.95.100.117.109.112.95.100.101.115.116.1]
    regsub {[?]} [lindex $temp_list 1] $oramsg(orahome) dest
    return $dest
}


proc USER_DUMP_DEST {} {
    # return the user_dump destination of the target database
    global oramsg
    set temp_list [oradbsnmp get rdbmsSrvParamCurrValue.$oramsg(oraindex).14.117.115.101.114.95.100.117.109.112.95.100.101.115.116.1]
    regsub {[?]} [lindex $temp_list 1] $oramsg(orahome) dest
    return $dest
}


proc ARCHIVE_DEST {} {
    # return the archive destination of the target database
    global oramsg
    set temp_list [oradbsnmp get rdbmsSrvParamCurrValue.$oramsg(oraindex).16.108.111.103.95.97.114.99.104.105.118.101.95.100.101.115.116.1]
    regsub {[?]} [lindex $temp_list 1] $oramsg(orahome) arch_dest
    return $arch_dest
}


proc getagentstatus {} {
    # returns status of agent 
    if { [catch {exec [AGENTCTL] status -ping} buffer] } {
       if {[string first [string trimright [msgtxt1 [NETWORK] NMS 383] "\n"] $buffer] >= 0 } {
           return [list 0 [string trimright [msgtxt1 [NETWORK] NMS 383] "\n"]]
       }
        return [list -1 $buffer]
    }
    if {[set index [string first [msgtxt1 [NETWORK] NMS 481] $buffer]] >= 0 } {
        return [list 1 [string range $buffer $index end]]
    } else {
         return [list -1 ""]
    } 
}


proc startagent {} {
    # starts the agent
    if { [lindex [getagentstatus] 0] != 1 } {
	if { [catch {exec [AGENTCTL] start >/dev/null} buffer] } {
	    return [list -1 $buffer] 
	}
    }  
    return [list 0 [string trimright [msgtxt1 [NETWORK] nms 411] "\n"]]
}


proc stopagent {} {
    # stops the agent
    if { [lindex [getagentstatus] 0] != 0 } {
	if { [catch {exec [AGENTCTL] stop >/dev/null} buffer] } {
	    return [list -1 $buffer] 
	}
    }  
    return [list 0 [string trimright [msgtxt1 [NETWORK] nms 409] "\n"]]
}


proc ORATCL_DEBUG {message} {
    # trace macro for oratcl scripts 
    global env
    global tcl_platform
    set trace_level [lindex [getagenttraceinfo] 0] 
    if { ($trace_level != 0) && ([string toupper $trace_level] != "OFF") } {
	if { [lindex [getagenttraceinfo] 1] == "" } {
           if {$tcl_platform(os) == "VMS"} {
                append fn [NETWORK_TRACE_DIR] oratcl.trc
            } else {
	        append fn [NETWORK_TRACE_DIR] "/" oratcl.trc
            }
	} else {
            if {$tcl_platform(os) == "VMS"} {
                append fn [lindex [getagenttraceinfo] 1] oratcl.trc
            } else {
	        append fn [lindex [getagenttraceinfo] 1] "/" oratcl.trc
            }
	}
        catch {set fh [open $fn a]
	  puts $fh $message
          flush $fh
        }
        # close the file only if it was successfully openned
        if { [info exists fh] } {
	    close $fh
        }
    }
}

global orainstargs

proc ListPackages {} {

    set code [catch {set home [checkHomeDefined]} msg]
		if { $code } { error $msg }

    set code [catch {loadOsm} msg]
	  if { $code } { error $msg }

    set code [catch {set packageList [lindex [getPackageInfo] 0]} msg]
	  if {$code} {
	  	error $msg
	  }

		puts [agentGetMessage 150 "------------   ----------------   -------------   -----------------"] 
		puts [agentGetMessage 151 "Package name   Operating system   Response file   Number of products"]
		puts [agentGetMessage 150 "------------   ----------------   -------------   -----------------"] 
    if { [llength $packageList] == 0} {
				 puts [agentGetMessage 152 "          *************     No Packages     *************          "]
				 puts [agentGetMessage 150 "------------   ----------------   -------------   -----------------"] 
				 return
		}

    foreach packageinfo $packageList {
				set packageFullName [lindex $packageinfo 0]
				set packageDir [getPackageDir]/[lindex $packageinfo 1]
				set packageDir [getPackageDir]/[lindex $packageinfo 1]
				set packageFileName [lindex [generatePackageName $packageFullName ] 1]
				set rspFileName $packageDir/$packageFileName.rsp
        set os [lindex $packageinfo 2]
				set noPrds [llength [lindex $packageinfo 4]]
		    puts "$packageFullName   $os   $rspFileName   $noPrds"
    }
		puts [agentGetMessage 150 "------------   ----------------   -------------   -----------------"] 
}

proc loadOsm {} {
		set loadedPackages [package names]
		if { [lsearch -exact $loadedPackages osm] == -1 } {
				set code [catch {package require osm 1} msg]
				if { $code } { error $msg }
		}
}

proc GenerateResponse { pkgName {backupRspFile ""} } {

	set code [catch { serializeJob } msg]
	if {$code} { error $msg }

	set code [catch {GenerateResponse_helper $pkgName $backupRspFile} msg]
	if {$code} {
		unserializeJob
		error $msg
	}
}

proc GenerateResponse_helper { pkgName {backupRspFile ""} } {

    set code [catch {set home [checkHomeDefined]} msg]
		if { $code } { error $msg }

    set code [catch {loadOsm} msg]
		if { $code } { error $msg }

    set packageID [generatePackageName $pkgName]
		set name [lindex $packageID 0]
		puts [agentGetMessage 153 "Initiating Response file generation ..."]
		set fileName [lindex $packageID 1]
		set code [catch {getPackageInfo $name} msg]
		if { $code } { error $msg }

		set package  [lindex [lindex $msg 0] 0] ;
	  set packageDir [lindex $package 1]
	  set location [getPackageDir]/$packageDir
    set inpRspFile $location/$fileName.rsp
    set outRspFile $location/$fileName.rsp

    if { $backupRspFile != "" } {
	      if { [file exists $backupRspFile] == 1 } {
				    error [agentGetMessage 154 "Backup file already exists"]
		    }

	      if { [file exists $inpRspFile] == 0 } {
				    error [agentGetMessage 155 "Default response file does not exist"]
		    }

        set code [catch {set srcfd [open $inpRspFile r]} msg ]
				if { $code } { error $msg }

        set code [catch {set destfd [open $backupRspFile w]} msg]
				if { $code } { error $msg }

				while { [gets $srcfd buffer] >= 0 } {
					 puts $destfd $buffer
				}
				close $srcfd
				close $destfd
		}

		set code [catch {generateStagingArea [list $pkgName] "" -interactive -respfile $inpRspFile -record $outRspFile } msg]
		if { $code } {
				error $msg
		}
		cd $home/orainst
}

proc generateStagingArea {name context args} {
   global orainstargs

   set packageID [generatePackageName $name]
   set name [lindex $packageID 0]
   set fileName [lindex $packageID 1]
   set instCmd [createInstallerArgs $args]
   set code [catch "getPackageInfo {$name} " msg]
   if {$code != 0} {
       error $msg
   } 
   set package  [lindex [lindex $msg 0] 0] ; 
	 set packageDir [lindex $package 1]
	 set location [getPackageDir]/$packageDir
   set os [lindex $package 2] ; 
   set relNum 1
   set prodlist [lindex $package 4] ; 
   set packagePrdFiles [lindex $package 5] ; 
   if {[lsearch -exact $instCmd /rspsrc] == -1} {
	set respFile "/rspsrc $location/$fileName.rsp"        
   } else {
	   set respFile ""
   }
   set code [catch {unArchivePackage "$location/$fileName" "$location"} tarmsg]
   if {$code != 0} {
	error $tarmsg
   }

  set prdFileNum 0
	set prdFile [lindex $packagePrdFiles $prdFileNum]
	set prdPath $location/$relNum/$prdFile

	set orainstargs "$instCmd $respFile /prd $prdPath /install [compileForcedInstallationList $prodlist]"

	puts [agentGetMessage 156 "Execute the following 2 commands to complete response file generation"]
	puts "./orainst \$orainstargs"
	puts "EndGenerateResponse $name"
}

proc EndGenerateResponse { name } {

	set code [catch {EndGenerateResponse_helper $name} msg]
	if {$code} {
		unserializeJob
		error $msg
	}

	set code [catch { unserializeJob } msg]
	if {$code} { error $msg }
}

proc EndGenerateResponse_helper { name } {

    set code [catch {set home [checkHomeDefined]} msg]
		if { $code } { error $msg }

    set code [catch {loadOsm} msg]
		if { $code } { error $msg }

   set packageID [generatePackageName $name]
   set name [lindex $packageID 0]
   set fileName [lindex $packageID 1]
   set code [catch "getPackageInfo {$name} " msg]
   set relNum 1
   if {$code != 0} {
       error $msg
   } 
   set package  [lindex [lindex $msg 0] 0] ; 
	 set packageDir [lindex $package 1]
	 set location [getPackageDir]/$packageDir

	 delDir $location/$relNum

   cd $location
   set code [catch {compressPackage $location/$fileName} msg]
   if {$code != 0} {
       error $msg
   }
   puts [agentGetMessage 157 "Response file generation complete"]
}

proc checkHomeDefined { } {
	
	 set code [catch {set home [AGENT_ORACLE_HOME]} msg]
	 if { $code } { error "ORACLE_HOME not defined" }
	 return $home
}

proc createInstallerArgs cmd {
        set out {/c}
        set code [lsearch -exact $cmd -interactive]
        if {$code  == -1} {
           lappend out /silent
        }
 
        set code [lsearch -exact $cmd -respfile]
        if {$code != -1} {
                set tmpname [lindex $cmd [expr $code + 1]]
                lappend out /rspsrc $tmpname
        }
        set code [lsearch -exact $cmd -record]
        if {$code != -1} {
                set tmpname [lindex $cmd [expr $code + 1]]
                lappend out /rspdest $tmpname
        }
        return $out
}


proc setOracleEnvironment {} {
    global tcl_platform

    if {$tcl_platform(platform) == "unix"} {
	global env setOracleEnvironment_OracleHome setOracleEnvironment_Path
	global setOracleEnvironment_Ld_Library_Path
        global setOracleEnvironment_Ora_Nls
        global setOracleEnvironment_Ora_Nls32
        global setOracleEnvironment_Ora_Nls33
	
	if { [info exists env(ORACLE_HOME)] } {
	    set setOracleEnvironment_OracleHome  $env(ORACLE_HOME)
	    set env(ORACLE_HOME) [ORACLE_HOME]
	}
	
        if {$tcl_platform(platform) == "HP-UX"} {
           if { [info exists env(SHLIB_PATH)] } {
             set setOracleEnvironment_Ld_Library_Path $env(SHLIB_PATH)
             set env(SHLIB_PATH) [ORACLE_HOME]/lib:$env(SHLIB_PATH)
           }
        } else {
	   if { [info exists env(LD_LIBRARY_PATH)] } {
	      set setOracleEnvironment_Ld_Library_Path $env(LD_LIBRARY_PATH)
	      set env(LD_LIBRARY_PATH) [ORACLE_HOME]/lib:$env(LD_LIBRARY_PATH)
	   }
        }
	if { [info exists env(PATH)] } {
	    set setOracleEnvironment_Path $env(PATH)
	    set env(PATH) [ORACLE_HOME]/bin:$env(PATH)
	}

        #
        # Unset ORA_NLS* variables so that they default to the correct
        # value. See bug 644607 for details
        #
	if { [info exists env(ORA_NLS)] } {
	    set setOracleEnvironment_Ora_Nls $env(ORA_NLS)
            unset env(ORA_NLS)
	} else {
	    set setOracleEnvironment_Ora_Nls ""
        }

	if { [info exists env(ORA_NLS32)] } {
	    set setOracleEnvironment_Ora_Nls32 $env(ORA_NLS32)
            unset env(ORA_NLS32)
	} else {
	    set setOracleEnvironment_Ora_Nls32 ""
        }

	if { [info exists env(ORA_NLS33)] } {
	    set setOracleEnvironment_Ora_Nls33 $env(ORA_NLS33)
            unset env(ORA_NLS33)
	} else {
	    set setOracleEnvironment_Ora_Nls33 ""
        }
    }
}

proc restoreOracleEnvironment {} {
    global tcl_platform

    if {$tcl_platform(platform) == "unix"} {
	global env setOracleEnvironment_OracleHome setOracleEnvironment_Path
	global setOracleEnvironment_Ld_Library_Path
        global setOracleEnvironment_Ora_Nls
        global setOracleEnvironment_Ora_Nls32
        global setOracleEnvironment_Ora_Nls33

	if { [info exists env(ORACLE_HOME)] } {
	    set env(ORACLE_HOME) $setOracleEnvironment_OracleHome
	}

        if {$tcl_platform(platform) == "HP-UX"} {
            if { [info exists env(SHLIB_PATH)] } {
               set env(SHLIB_PATH) $setOracleEnvironment_Ld_Library_Path
            }
        } else {
  	   if { [info exists env(LD_LIBRARY_PATH)] } {
	      set env(LD_LIBRARY_PATH) $setOracleEnvironment_Ld_Library_Path 
	   }
        }

	if { [info exists env(PATH)] } {
	    set env(PATH) $setOracleEnvironment_Path 
	}

        #
        # Restore ORA_NLS<*> values
        #
        if { $setOracleEnvironment_Ora_Nls != ""} {
	    set env(ORA_NLS) $setOracleEnvironment_Ora_Nls
        }

        if {$setOracleEnvironment_Ora_Nls32 != ""} {
	    set env(ORA_NLS32) $setOracleEnvironment_Ora_Nls32
        }

        if {$setOracleEnvironment_Ora_Nls33 != ""} {
	    set env(ORA_NLS33) $setOracleEnvironment_Ora_Nls33
        }
    }
}

proc PINGCMD {node packet_size} {    
    set total_time 0
    set avg_time 0 

    #PING *node*: 64 data bytes
    #72 bytes from *node* (138.1.17.129): icmp_seq=0. time=88. ms
    #72 bytes from *node* (138.1.17.129): icmp_seq=1. time=94. ms
    #72 bytes from *node* (138.1.17.129): icmp_seq=2. time=91. ms
    #72 bytes from *node* (138.1.17.129): icmp_seq=3. time=99. ms
    #----*node* PING Statistics----
    #4 packets transmitted, 4 packets received, 0% packet loss
    #round-trip (ms)  min/avg/max = 88/93/99

    set ping_iterations 4
    set ping_args "-s $node $packet_size $ping_iterations"
    set ping_search {time[<|=]}
    set ping_regexp {%d %s %s %s %[()0-9.]: %[a-z_]=%d. time%[=<]%d}
    
    set code [catch {OS_SHELL_EXEC "ping $ping_args"} output]
    
    if { $code == 0 } {
	set output_list [split $output \n]
	for {set i 0} {$i < [llength $output_list]} {incr i} {
	    if { [regexp -nocase $ping_search [lindex $output_list $i]] } {
		set line [lindex $output_list $i]
		if { [scan $line $ping_regexp a b c d e f g h t] != 9} {
		    return -code 1 $output
		} else {	
		    set total_time [expr $total_time+$t]
		    set t ""
		}
	    }   
	}
	if { $total_time > 0 } {
	    set avg_time [expr $total_time/$ping_iterations]
	}
	set output $avg_time        
    }
    return -code $code $output
}

proc IPLOOKUP {node} {    
    #Server: foo.us.oracle.com
    #Address: 138.1.1.138
    #
    #Name:  *node*
    #Address:  138.1.17.129
    
    set ns_args "$node"
    set ns_search "Name:"
    set ns_regexp {%[a-zA-Z]:%[0-9., ]}
    
    set code [catch {OS_SHELL_EXEC "nslookup $ns_args"} output]
    
    if { $code == 0 } {
	set output_list [split $output \n]
	for {set i 0} {$i < [llength $output_list]} {incr i} {
	    if { [regexp -nocase $ns_search [lindex $output_list $i]] } {
		set ip_line [lindex $output_list [expr $i+1]]
		if {[scan $ip_line $ns_regexp a ip_address] != 2} {
		    return -code 1 $output
		} else {
		    set output [string trim $ip_address]
		}
	    }
	}
    }
    return -code $code $output
}
proc joboutfile { ftype id } {
    return [file join [AGENT_ORACLE_HOME] network agent jobout $ftype$id]
}

proc AGENTCTL {} {
  
   set agentctlexec ""
   set agentctlexec [concatname [list [AGENT_ORACLE_HOME] bin agentctl]]

   return $agentctlexec
}


proc EMDROOT_NOTEXIST_MSG {} {

    global output
    lappend output "Please use the following format in emtab at $tabfile."
    lappend output "DEFAULT=<EMDROOT>"
}

proc EMCTL { emdroot } {
    return "$emdroot/bin/emctl"
}

proc EMDROOT_FUNC {} {
    global SCRIPT_FAIL
    global tcl_platform
    global output

    set osname $tcl_platform(os)
    if { [string compare $osname "SunOS"] == 0 } {
        set tabfile "/var/opt/oracle/emtab"
    }
    if { [string compare $osname "HP-UX"] == 0} {
        set tabfile "/etc/emtab"
    }
    
    #check that emtab file exists
    if { [file exists $tabfile] == 0 } {
        # emtab file does not exist
        lappend output "File $tabfile, does not exist."
        return $SCRIPT_FAIL
    }
    
    #make sure that the emtab file is readable
    if { [file readable $tabfile] == 0 } {
        #emdtab file does not have read permissions
        lappend output "File $tabfile is not readable."
        return $SCRIPT_FAIL
    }

    if [catch {open $tabfile r} fd] {
        lappend output  "Could not open file $tabfile."
        return $SCRIPT_FAIL
    }
    foreach line [split [read $fd] \n] {
        if {[string compare -length 8 "DEFAULT=" $line] == 0} {
            if {[string length $line] == 8} {
                #emtab file contains DEFAULT= string but nothing after it
                lappend output "emtab file contains DEFAULT= without value."
                return $SCRIPT_FAIL
            }
            
        }
        
        set emdroot [string trim [string range $line 8 end]]
        return $emdroot
    }
}

