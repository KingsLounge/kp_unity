// Decompiled by Jad v1.5.8g. Copyright 2001 Pavel Kouznetsov.
// Jad home page: http://www.kpdus.com/jad.html
// Decompiler options: packimports(3) 
// Source File Name:   Bridge.java

package com.pingak9.nativepopup;
import android.app.AlertDialog
import android.app.DatePickerDialog
import android.app.TimePickerDialog
import android.content.DialogInterface
import android.widget.TimePicker
import com.unity3d.player.UnityPlayer
import java.util.*

fun ShowDialogNeutral(
        title: String?,
        message: String?,
        accept: String?,
        neutral: String?,
        decline: String?
    ) {
        DismissCurrentAlert()
        alertDialog = AlertDialog.Builder(UnityPlayer.currentActivity).create()
        alertDialog?.setTitle(title)
        alertDialog?.setMessage(message)
        alertDialog?.setButton(
            accept,
            DialogInterface.OnClickListener { dialog, which ->
                UnityPlayer.UnitySendMessage(
                    "MobileDialogNeutral",
                    "OnAcceptCallBack",
                    "0"
                )
            }
        )
        alertDialog?.setButton2(
            neutral,
            DialogInterface.OnClickListener { dialog, which ->
                UnityPlayer.UnitySendMessage(
                    "MobileDialogNeutral",
                    "OnNeutralCallBack",
                    "1"
                )
            }
        )
        alertDialog?.setButton3(
            decline,
            DialogInterface.OnClickListener { dialog, which ->
                UnityPlayer.UnitySendMessage(
                    "MobileDialogNeutral",
                    "OnDeclineCallBack",
                    "2"
                )
            }
        )
        alertDialog?.show()
    }

    fun ShowDialogConfirm(title: String?, message: String?, yes: String?, no: String?) {
        DismissCurrentAlert()
        alertDialog = AlertDialog.Builder(UnityPlayer.currentActivity).create()
        alertDialog?.setTitle(title)
        alertDialog?.setMessage(message)
        alertDialog?.setButton(
            yes,
            DialogInterface.OnClickListener { dialog, which ->
                UnityPlayer.UnitySendMessage(
                    "MobileDialogConfirm",
                    "OnYesCallBack",
                    "0"
                )
            }
        )
        alertDialog?.setButton2(
            no,
            DialogInterface.OnClickListener { dialog, which ->
                UnityPlayer.UnitySendMessage(
                    "MobileDialogConfirm",
                    "OnNoCallBack",
                    "1"
                )
            }
        )
        alertDialog?.show()
    }

    fun ShowDialogInfo(title: String?, message: String?, ok: String?) {
        DismissCurrentAlert()
        alertDialog = AlertDialog.Builder(UnityPlayer.currentActivity).create()
        alertDialog?.setTitle(title)
        alertDialog?.setMessage(message)
        alertDialog?.setButton(
            ok,
            DialogInterface.OnClickListener { dialog, which ->
                UnityPlayer.UnitySendMessage(
                    "MobileDialogInfo",
                    "OnOkCallBack",
                    "0"
                )
            }
        )
        alertDialog?.show()
    }

    fun DismissCurrentAlert() {
        if (alertDialog != null) alertDialog!!.hide()
    }

    private var pickedDate = ""
    fun ShowDatePicker(year: Int, month: Int, day: Int) {
        pickedDate = "Cancel"
        val datePickerDialog =
            DatePickerDialog(UnityPlayer.currentActivity, { view, year, monthOfYear, dayOfMonth ->
                pickedDate = String.format(
                    "%d-%d-%d %d:%d:%d", *arrayOf<Any>(
                        Integer.valueOf(year),
                        Integer.valueOf(monthOfYear + 1),
                        Integer.valueOf(dayOfMonth),
                        Integer.valueOf(0),
                        Integer.valueOf(0),
                        Integer.valueOf(0)
                    )
                )
            }, year, month - 1, day)
        datePickerDialog.setOnDismissListener(mOnDismissListener)
        datePickerDialog.show()
    }

    private val mOnDismissListener = DialogInterface.OnDismissListener {
        UnityPlayer.UnitySendMessage(
            "MobileDateTimePicker",
            "PickerClosedEvent",
            pickedDate
        )
    }

    fun ShowTimePicker() {
        val c = Calendar.getInstance()
        val hour = c[11]
        val minute = c[12]
        pickedDate = "Cancel"
        val timePickerDialog = TimePickerDialog(
            UnityPlayer.currentActivity,
            { view: TimePicker?, hourOfDay: Int, minute1: Int ->
                val year = c[1]
                val day = c[5]
                val month = c[2]
                pickedDate = String.format(
                    "%d-%d-%d %d:%d:%d", *arrayOf<Any>(
                        Integer.valueOf(year),
                        Integer.valueOf(month + 1),
                        Integer.valueOf(day),
                        Integer.valueOf(hourOfDay),
                        Integer.valueOf(minute1),
                        Integer.valueOf(0)
                    )
                )
            },
            hour,
            minute,
            true)
        timePickerDialog.setOnDismissListener(mOnDismissListener)
        timePickerDialog.show()
    }

    var alertDialog: AlertDialog? = null