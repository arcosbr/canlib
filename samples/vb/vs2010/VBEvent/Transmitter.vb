Public Class Transmitter

    Dim ChannelHandle As Integer

    Private Sub DisplayError(ByVal errorCode As Kvaser.CanLib.Canlib.canStatus, ByVal msgTitle As String)
        Dim msgDescr As String = " "
        Dim status As Kvaser.CanLib.Canlib.canStatus

        If (errorCode < Kvaser.CanLib.Canlib.canStatus.canOK) Then
            status = Kvaser.CanLib.Canlib.canGetErrorText(errorCode, msgDescr)
            If (status <> Kvaser.CanLib.Canlib.canStatus.canOK) Then
                msgDescr = "Unknown error"
            End If
            MessageBox.Show(msgTitle, msgTitle + " returned error code " + Str(errorCode) _
                         + " - " + msgDescr,
                         MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
    End Sub

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' restrict control entry
        StandardRB.Checked = True

        ' Add any initialization after the InitializeComponent() call.
        Dim status As Kvaser.CanLib.Canlib.canStatus

        'Open the channel (The flags will allow a virtual channel to be used)
        ChannelHandle = Kvaser.CanLib.Canlib.canOpenChannel(1, Kvaser.CanLib.Canlib.canOPEN_ACCEPT_VIRTUAL)
        DisplayError(ChannelHandle, "canOpenChannel")

        'Set the bit params (In this case 250 kbits/sec)
        status = Kvaser.CanLib.Canlib.canSetBusParams(ChannelHandle, Kvaser.CanLib.Canlib.canBITRATE_250K, 0, 0, 0, 0)
        DisplayError(status, "canSetBusParams")

        'Make the channel active
        status = Kvaser.CanLib.Canlib.canBusOn(ChannelHandle)
        DisplayError(status, "canBusOn")

    End Sub

    Private Sub SendButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SendButton.Click
        Dim id As Integer = 0
        Dim data As Byte() = New Byte(7) {}
        Dim dlc As Integer = 0
        Dim flag As Integer = 0
        Dim status As Kvaser.CanLib.Canlib.canStatus

        id = Val(CanIdTB.Text)
        dlc = Val(DlcTB.Text)
        data(0) = Val(DataTB1.Text)
        data(1) = Val(DataTB2.Text)
        data(2) = Val(DataTB3.Text)
        data(3) = Val(DataTB4.Text)
        data(4) = Val(DataTB5.Text)
        data(5) = Val(DataTB6.Text)
        data(6) = Val(DataTB7.Text)
        data(7) = Val(DataTB8.Text)
        If (StandardRB.Checked) Then
            flag = Kvaser.CanLib.Canlib.canMSG_STD
        Else
            flag = Kvaser.CanLib.Canlib.canMSG_EXT
        End If

        status = Kvaser.CanLib.Canlib.canWrite(ChannelHandle, id, data, dlc, flag)

    End Sub

    Private Sub Transmitter_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Dim status As Kvaser.CanLib.Canlib.canStatus

        'Make the channel inactive
        status = Kvaser.CanLib.Canlib.canBusOff(ChannelHandle)
        DisplayError(status, "canBusOff")

        status = Kvaser.CanLib.Canlib.canClose(ChannelHandle)
        DisplayError(status, "canClose")

    End Sub
End Class