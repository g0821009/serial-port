' 2014.09.10.1323 ver1.01 ターミネータに関わらずTABを最後に入力
' 2014.09.12.1323 ver1.02 ポートリスト一覧が取得できない不具合を修正

Public Class Form1
    Dim weit As String
    Dim appPath As String = System.IO.Directory.GetCurrentDirectory() & "\"
    Dim fileName As String = ".serial-port.ini"
    Dim setting_wf As IO.StreamWriter
    Dim Encode As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift-JIS")

    Private Sub createListBox2()
        ListBox2.Items.Add(250)
        ListBox2.Items.Add(500)
        ListBox2.Items.Add(1000)
        ListBox2.Items.Add(1500)
    End Sub

    Private Function importSetting()
        Dim settingFile As IO.StreamReader
        Dim sa As New ArrayList
        Dim stBuffer As String

        Debug.WriteLine("import Setting start.")
        Try
            settingFile = New IO.StreamReader(appPath + fileName, Encode, False)
            While (settingFile.Peek() >= 0)
                stBuffer = settingFile.ReadLine()
                Debug.WriteLine(stBuffer)
                sa.Add(stBuffer)
            End While
            settingFile.Close()
        Catch ex As Exception
            Debug.WriteLine("file open error:")
            Debug.WriteLine(ex)
        End Try
        Return sa
    End Function

    Private Sub exportSetting(ByVal settingLines As String)
        Dim settingFile As IO.StreamWriter
        Try
            '既に存在するテキストに追加する場合は第２引数をTrueにする。
            settingFile = New IO.StreamWriter(appPath & fileName, False, Encode)
            settingFile.Write(settingLines)
            settingFile.Close()
        Catch ex As Exception
            Debug.WriteLine("Error:")
            Debug.WriteLine(ex)
        End Try
        Debug.WriteLine("save Settings")
        Debug.WriteLine(appPath & fileName)
        Debug.WriteLine(settingLines)
    End Sub

    '文字コードにShiftJISを指定。(UTF8の場合は指定不要)
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim portnames As String()
        Dim portname As String
        Dim settingArray As New ArrayList

        createListBox2()

        '        portnames = SerialPort.GetPortNames()
        '       Debug.WriteLine("port names:")
        '      Debug.WriteLine(portnames)

        'If IsArray(portnames) Then
        'Debug.WriteLine("port name is Nothing")
        'Else
        For Each portname In My.Computer.Ports.SerialPortNames
            ListBox1.Items.Add(portname)
        Next portname
        'End If

        settingArray = importSetting()

        If settingArray.Count > 0 Then
            Dim setting_item() As String
            For Each setting_line As String In settingArray

                ' TODO delimiter error
                setting_item = setting_line.Split(":"c)
                Select Case setting_item(0)
                    Case "portname"
                        Label3.Text = setting_item(1)
                    Case "weit"
                        Label4.Text = setting_item(1)
                End Select
            Next
        Else
            Label3.Text = ""
            Label4.Text = "500"
        End If

        Try
            If (ListBox1.FindStringExact(Label3.Text) = ListBox.NoMatches) Then
                Debug.WriteLine("port name no matches in port list.")
                If Label3.Text = "" Then
                    SerialPort1.PortName = "COM1"
                    SerialPort1.Open()
                Else
                    SerialPort1.PortName = Label3.Text
                    SerialPort1.Open()
                End If
            Else
                SerialPort1.PortName = Label3.Text
                SerialPort1.Open()
            End If
        Catch ex As Exception
            Debug.WriteLine("error:")
            Debug.WriteLine(ex)
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub Form1_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed
        Dim settingLines = String.Empty
        settingLines &= "portname:" & SerialPort1.PortName & System.Environment.NewLine
        settingLines &= "weit:" & Label4.Text
        exportSetting(settingLines)
        Debug.WriteLine("finished")
        If SerialPort1.IsOpen = True Then   'ポートオープン済み
            SerialPort1.Close()              'ポートクローズ
        End If
    End Sub

    Private Sub ListBox1_Click(sender As Object, e As EventArgs) Handles ListBox1.Click
        If ListBox1.Items.Count = 0 Then

        Else
            If SerialPort1.IsOpen = True Then   'ポートオープン済み
                SerialPort1.Close()              'ポートクローズ
            End If
            SerialPort1.PortName = ListBox1.SelectedItem
            Label3.Text = SerialPort1.PortName
            SerialPort1.Open()
            Debug.WriteLine("open " + SerialPort1.PortName)
        End If
    End Sub

    Private Sub SerialPort1_DataReceived(sender As Object, e As IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        Dim received_strings As String
        Dim c As Char

        received_strings = SerialPort1.ReadLine
        For Each c In received_strings
            Debug.Write(c)
            If c = ChrW(Keys.Enter) Then
                SendKeys.SendWait(ChrW(Keys.Tab))
            Else
                SendKeys.SendWait(c)
            End If
            If c = vbTab Then
                System.Threading.Thread.Sleep(weit)
            End If
        Next
    End Sub

    Private Sub ListBox2_Click(sender As Object, e As EventArgs) Handles ListBox2.Click
        If ListBox2.Text <> "" Then
            weit = ListBox2.Text
            Label4.Text = weit
            Debug.WriteLine("weit is " + weit)
        End If
    End Sub
End Class
