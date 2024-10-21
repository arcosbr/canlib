Public Class Form1
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents InitButton As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents ReceivedRB As System.Windows.Forms.RadioButton
    Friend WithEvents WriteButton As System.Windows.Forms.Button
    Friend WithEvents ReadButton As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Write100Button As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents CardTypeButton As System.Windows.Forms.Button
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents TextBox3 As System.Windows.Forms.TextBox
    Friend WithEvents BitrateButton As System.Windows.Forms.Button
    Friend WithEvents Label10 As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.InitButton = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.ReceivedRB = New System.Windows.Forms.RadioButton()
        Me.WriteButton = New System.Windows.Forms.Button()
        Me.ReadButton = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Write100Button = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.TextBox3 = New System.Windows.Forms.TextBox()
        Me.BitrateButton = New System.Windows.Forms.Button()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.CardTypeButton = New System.Windows.Forms.Button()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'InitButton
        '
        Me.InitButton.Location = New System.Drawing.Point(40, 48)
        Me.InitButton.Name = "InitButton"
        Me.InitButton.Size = New System.Drawing.Size(75, 23)
        Me.InitButton.TabIndex = 0
        Me.InitButton.Text = "Initialize"
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(128, 48)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(128, 24)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Initializes the hardware "
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(104, 24)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(144, 40)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Sends a CAN message from channel 0"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(104, 112)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(136, 32)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "Indicates if there are any messages to read"
        '
        'ReceivedRB
        '
        Me.ReceivedRB.Location = New System.Drawing.Point(16, 112)
        Me.ReceivedRB.Name = "ReceivedRB"
        Me.ReceivedRB.Size = New System.Drawing.Size(80, 24)
        Me.ReceivedRB.TabIndex = 5
        Me.ReceivedRB.Text = "Recevied"
        '
        'WriteButton
        '
        Me.WriteButton.Location = New System.Drawing.Point(16, 32)
        Me.WriteButton.Name = "WriteButton"
        Me.WriteButton.Size = New System.Drawing.Size(75, 23)
        Me.WriteButton.TabIndex = 1
        Me.WriteButton.Text = "Write msg"
        '
        'ReadButton
        '
        Me.ReadButton.Location = New System.Drawing.Point(16, 152)
        Me.ReadButton.Name = "ReadButton"
        Me.ReadButton.Size = New System.Drawing.Size(75, 23)
        Me.ReadButton.TabIndex = 6
        Me.ReadButton.Text = "Read msg"
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(104, 152)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(136, 32)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Reads  the messages from the queue"
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(16, 192)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(72, 20)
        Me.TextBox1.TabIndex = 8
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(104, 192)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(128, 32)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Number of received messages"
        '
        'Label6
        '
        Me.Label6.Location = New System.Drawing.Point(104, 64)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(144, 40)
        Me.Label6.TabIndex = 12
        Me.Label6.Text = "Sends 100 CAN messages from channel 0 "
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Write100Button
        '
        Me.Write100Button.Location = New System.Drawing.Point(16, 72)
        Me.Write100Button.Name = "Write100Button"
        Me.Write100Button.Size = New System.Drawing.Size(75, 23)
        Me.Write100Button.TabIndex = 11
        Me.Write100Button.Text = "Write msg"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label6)
        Me.GroupBox1.Controls.Add(Me.Write100Button)
        Me.GroupBox1.Controls.Add(Me.TextBox1)
        Me.GroupBox1.Controls.Add(Me.Label5)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.ReceivedRB)
        Me.GroupBox1.Controls.Add(Me.WriteButton)
        Me.GroupBox1.Controls.Add(Me.ReadButton)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Location = New System.Drawing.Point(296, 16)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(256, 240)
        Me.GroupBox1.TabIndex = 10
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Sending and receiving msg"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label9)
        Me.GroupBox2.Controls.Add(Me.TextBox3)
        Me.GroupBox2.Controls.Add(Me.BitrateButton)
        Me.GroupBox2.Controls.Add(Me.Label10)
        Me.GroupBox2.Controls.Add(Me.Label8)
        Me.GroupBox2.Controls.Add(Me.TextBox2)
        Me.GroupBox2.Controls.Add(Me.CardTypeButton)
        Me.GroupBox2.Controls.Add(Me.Label7)
        Me.GroupBox2.Location = New System.Drawing.Point(24, 16)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(256, 240)
        Me.GroupBox2.TabIndex = 11
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Configurations"
        '
        'Label9
        '
        Me.Label9.Location = New System.Drawing.Point(104, 192)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(128, 24)
        Me.Label9.TabIndex = 18
        Me.Label9.Text = "Bitrate"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TextBox3
        '
        Me.TextBox3.Location = New System.Drawing.Point(16, 192)
        Me.TextBox3.Name = "TextBox3"
        Me.TextBox3.Size = New System.Drawing.Size(72, 20)
        Me.TextBox3.TabIndex = 17
        '
        'BitrateButton
        '
        Me.BitrateButton.Location = New System.Drawing.Point(16, 152)
        Me.BitrateButton.Name = "BitrateButton"
        Me.BitrateButton.Size = New System.Drawing.Size(75, 23)
        Me.BitrateButton.TabIndex = 15
        Me.BitrateButton.Text = "Bitrate"
        '
        'Label10
        '
        Me.Label10.Location = New System.Drawing.Point(104, 152)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(128, 24)
        Me.Label10.TabIndex = 16
        Me.Label10.Text = "Gets the bitrate"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label8
        '
        Me.Label8.Location = New System.Drawing.Point(104, 112)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(128, 24)
        Me.Label8.TabIndex = 14
        Me.Label8.Text = "Card type"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TextBox2
        '
        Me.TextBox2.Location = New System.Drawing.Point(16, 112)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(72, 20)
        Me.TextBox2.TabIndex = 13
        '
        'CardTypeButton
        '
        Me.CardTypeButton.Location = New System.Drawing.Point(16, 72)
        Me.CardTypeButton.Name = "CardTypeButton"
        Me.CardTypeButton.Size = New System.Drawing.Size(75, 23)
        Me.CardTypeButton.TabIndex = 1
        Me.CardTypeButton.Text = "Card type"
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(104, 72)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(128, 24)
        Me.Label7.TabIndex = 12
        Me.Label7.Text = "Gets the card type of channel 0"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Form1
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(584, 278)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.InitButton)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.GroupBox2)
        Me.Name = "Form1"
        Me.Text = "Kvaser VB Sample program"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Dim hnd0, hnd1 As Integer
    Dim can_status As Kvaser.CanLib.Canlib.canStatus

    Private Sub InitButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles InitButton.Click
        Dim obj_buf As Object

        Kvaser.CanLib.Canlib.canInitializeLibrary()

        Kvaser.CanLib.Canlib.canInitializeLibrary()

        hnd0 = Kvaser.CanLib.Canlib.canOpenChannel(0, 0)
        hnd1 = Kvaser.CanLib.Canlib.canOpenChannel(1, 0)

        Kvaser.CanLib.Canlib.canGetChannelData(0, Kvaser.CanLib.Canlib.canCHANNELDATA_CHANNEL_NAME, obj_buf)

        Console.WriteLine("{0}: {1}", obj_buf.GetType, obj_buf)

        Kvaser.CanLib.Canlib.canSetBusParams(hnd0, Kvaser.CanLib.Canlib.canBITRATE_125K, 0, 0, 0, 0)
        Kvaser.CanLib.Canlib.canSetBusParams(hnd1, Kvaser.CanLib.Canlib.canBITRATE_125K, 0, 0, 0, 0)

        Kvaser.CanLib.Canlib.canBusOn(hnd0)
        Kvaser.CanLib.Canlib.canBusOn(hnd1)

        Kvaser.CanLib.Canlib.canSetNotify(hnd1, Me.Handle, Kvaser.CanLib.Canlib.canNOTIFY_RX)
        Me.InitButton.Enabled = False

    End Sub

    Private Sub WriteButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WriteButton.Click
        Dim msg(6) As Byte
        Kvaser.CanLib.Canlib.canWrite(hnd0, 123, msg, 6, 0)


    End Sub

    Private Sub ReadButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReadButton.Click
        Dim id, dlc, flag, i As Integer
        Dim time As Long
        Dim msg(6) As Byte

        can_status = Kvaser.CanLib.Canlib.canStatus.canOK
        While can_status = Kvaser.CanLib.Canlib.canStatus.canOK
            can_status = Kvaser.CanLib.Canlib.canRead(hnd1, id, msg, dlc, flag, time)
            If (can_status = Kvaser.CanLib.Canlib.canStatus.canOK) Then
                i = i + 1
            End If
        End While
        Me.ReceivedRB.Checked = False
        Me.TextBox1.Text = i

    End Sub
    Protected Overrides Sub WndProc( _
       ByRef m As Message _
    )
        If m.Msg = Kvaser.CanLib.Canlib.WM__CANLIB Then
            Console.WriteLine("WM__CANLIB received")
            Me.ReceivedRB.Checked = True
        End If
        MyBase.WndProc(m)
    End Sub

    Private Sub Write100Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Write100Button.Click
        Dim msg(6) As Byte
        Dim i As Integer
        For i = 0 To 99
            Kvaser.CanLib.Canlib.canWrite(hnd0, 123, msg, 6, 0)
        Next i

    End Sub

    Private Sub CardTypeButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CardTypeButton.Click
        Dim devname As String

        devname = "Unknown"

        can_status = Kvaser.CanLib.Canlib.canIoCtl(hnd0, Kvaser.CanLib.Canlib.canIOCTL_GET_DEVNAME_ASCII, devname)
        Me.TextBox2.Text = devname


    End Sub

    Private Sub BitrateButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BitrateButton.Click
        Dim freq As Long
        Dim tseg1, tseg2, sjw, nosamp As Integer
        Kvaser.CanLib.Canlib.canGetBusParams(hnd0, freq, tseg1, tseg2, sjw, nosamp)
        Me.TextBox3.Text = freq
    End Sub
End Class
