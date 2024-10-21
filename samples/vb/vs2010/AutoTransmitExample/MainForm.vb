Public Class MainForm
    Dim ChannelHandle As Integer
    Dim ObjBufHandle As Integer
    Dim FirstTime As Boolean = True
    Private Sub CleanAndExit()
        Dim status As Kvaser.CanLib.Canlib.canStatus

        'Stop the buffer from transmitting
        status = Kvaser.CanLib.Canlib.canObjBufDisable(ChannelHandle, ObjBufHandle)
        status = Kvaser.CanLib.Canlib.canObjBufFree(ChannelHandle, ObjBufHandle)
        status = Kvaser.CanLib.Canlib.canBusOff(ChannelHandle)
        status = Kvaser.CanLib.Canlib.canClose(ChannelHandle)

        'close the application
        Application.Exit()

    End Sub
    Private Sub DisplayErrorMsg(ByVal funcName As String, ByVal errorCode As Kvaser.CanLib.Canlib.canStatus)
        Dim newText As String = " "
        Dim outText As String = " "
        Dim status As Integer

        If (errorCode < Kvaser.CanLib.Canlib.canStatus.canOK) Then

            status = Kvaser.CanLib.Canlib.canGetErrorText(errorCode, newText)
            outText = "Function returned with error code " + Str(errorCode) _
                      + ":  " + newText + "."

            MessageBox.Show(outText, funcName + " Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation)

            'Shutdown the application
            CleanAndExit()
        End If

    End Sub

    Private Sub StartButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StartButton.Click
        Dim status As Kvaser.CanLib.Canlib.canStatus

        'Get a handle to the physical channel
        ChannelHandle = Kvaser.CanLib.Canlib.canOpenChannel(0, 0)
        DisplayErrorMsg("canOpenChannel", ChannelHandle)

        'Set the bit params (In this case 250 kbits/sec)
        status = Kvaser.CanLib.Canlib.canSetBusParams(ChannelHandle, Kvaser.CanLib.Canlib.canBITRATE_250K, 0, 0, 0, 0)
        DisplayErrorMsg("canSetBusParams", status)

        'Try to get a message object buffer
        ObjBufHandle = Kvaser.CanLib.Canlib.canObjBufAllocate(ChannelHandle, Kvaser.CanLib.Canlib.canObjBufType.PERIODIC_TX)

        DisplayErrorMsg("canObjBufAllocate", ObjBufHandle)

        'Set period on message buffer
        status = Kvaser.CanLib.Canlib.canObjBufSetPeriod(ChannelHandle, ObjBufHandle, Val(PeriodMTB.Text) * 1000)
        DisplayErrorMsg("canObjBufSetPeriod", status)

        'Make the handle active
        status = Kvaser.CanLib.Canlib.canBusOn(ChannelHandle)
        DisplayErrorMsg("canBusOn", status)

    End Sub

    Private Sub UpdateButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UpdateButton.Click
        Dim status As Kvaser.CanLib.Canlib.canStatus
        Dim id As Integer
        Dim dlc As Integer
        Dim flags As Integer
        Dim msg(7) As Byte

        'Get the data from the screen
        id = IdentifierNUD.Value
        dlc = DlcNUD.Value
        msg(0) = Data0NUD.Value
        msg(1) = Data1NUD.Value
        msg(2) = Data2NUD.Value
        msg(3) = Data3NUD.Value
        msg(4) = Data4NUD.Value
        msg(5) = Data5NUD.Value
        msg(6) = Data6NUD.Value
        msg(7) = Data7NUD.Value

        If (StandardIdRB.Checked) Then
            flags = Kvaser.CanLib.Canlib.canMSG_STD
        Else
            flags = Kvaser.CanLib.Canlib.canMSG_EXT
        End If

        'Change the object buffers contents
        status = Kvaser.CanLib.Canlib.canObjBufWrite(ChannelHandle, ObjBufHandle, id, msg, dlc, flags)
        DisplayErrorMsg("canObjBufWrite", status)

        If (FirstTime) Then
            'Enable the message buffer to start transmitting
            status = Kvaser.CanLib.Canlib.canObjBufEnable(ChannelHandle, ObjBufHandle)
            DisplayErrorMsg("canObjBufEnable", status)
            FirstTime = False
        End If
    End Sub

    Private Sub StopButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StopButton.Click
        Dim status As Kvaser.CanLib.Canlib.canStatus

        'Stop the buffer from transmitting
        status = Kvaser.CanLib.Canlib.canObjBufDisable(ChannelHandle, ObjBufHandle)
        DisplayErrorMsg("canObjBufDisable", status)

        'Free the buffer
        status = Kvaser.CanLib.Canlib.canObjBufFree(ChannelHandle, ObjBufHandle)
        DisplayErrorMsg("canObjBufFree", status)

        'Turn the handle off
        status = Kvaser.CanLib.Canlib.canBusOff(ChannelHandle)
        DisplayErrorMsg("canBusOff", status)

        'Free the handle
        status = Kvaser.CanLib.Canlib.canClose(ChannelHandle)
        DisplayErrorMsg("canClose", status)

        FirstTime = True
    End Sub

    Private Sub MainForm_Load_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Prepare the library for use
        Kvaser.CanLib.Canlib.canInitializeLibrary()
    End Sub
End Class
