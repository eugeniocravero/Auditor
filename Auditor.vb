
Public Class Auditor
    Private Tablas As DataTable

    Private Sub cmdAnterior3_Click()
        TabMain.SelectedIndex = 1
    End Sub

    Private Sub cmdAnterior2_Click()
        TabMain.SelectedIndex = 0
    End Sub

    Private Sub cmdCerrar_Click()
        End
    End Sub


    Private Sub Auditor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        Me.ddlProveedor.Items.Add(New Combo_item(1, "DB2 for i V7R1"))
        Me.ddlProveedor.Items.Add(New Combo_item(2, "SQL Server"))


    End Sub

    Private Sub btnSiguiente_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSiguiente.Click

        Try
            If Me.txtBaseDatosDestino.Text = "" Then
                Me.txtBaseDatosDestino.Text = Me.txtBaseDatos.Text
            End If

            Select Case (Me.ddlProveedor.SelectedIndex.ToString())
                Case "0"
                    Dim DB2 As New Acceso_DB2(Me.txtBaseDatos.Text, Me.txtServidor.Text, Me.txtusuario.Text, Me.txtPassword.Text, Me.txtBaseDatosDestino.Text)
                    Tablas = DB2.EjecutarConsulta("SELECT 0 as id, TABLE_NAME AS NAME from SYSIBM.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='" & Me.txtBaseDatos.Text & "' ORDER BY TABLE_NAME")

                Case "1"
                    Dim DB As New Acceso(Me.txtBaseDatos.Text, Me.txtServidor.Text, Me.txtusuario.Text, Me.txtPassword.Text, Me.txtBaseDatosDestino.Text)
                    Tablas = DB.EjecutarConsulta("Select name, id from SysObjects where xtype='U' order by name")

            End Select




            lstTablas.Items.Clear()
            Dim dtr As DataRow
            For Each dtr In Tablas.Rows
                Dim itm As New ListViewItem
                itm.Text = dtr("name").ToString.Trim()
                itm.Tag = dtr("id").ToString
                lstTablas.Items.Add(itm)
            Next
            TabMain.SelectedIndex = 1
            Exit Sub

        Catch er As Exception

            MsgBox(er.Message, vbExclamation)
            TabMain.SelectedIndex = 0
        Finally

        End Try

    End Sub

    Private Sub btnSiguiente2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSiguiente2.Click
        Try
            Select Case (Me.ddlProveedor.SelectedIndex.ToString())
                Case "0"
                    Dim DB2 As New Acceso_DB2(Me.txtBaseDatos.Text, Me.txtServidor.Text, Me.txtusuario.Text, Me.txtPassword.Text, Me.txtBaseDatosDestino.Text)
                    txtSQL.Text = ""
                    Dim dr As DataRow = Nothing
                    Dim itm As System.Windows.Forms.ListViewItem

                    For Each itm In lstTablas.CheckedItems
                        txtSQL.Text &= DB2.Get_Script(itm.Text, Me.chkAlta.Checked, Me.chkBaja.Checked, Me.chkModificacion.Checked)
                    Next
                    TabMain.SelectedIndex = 2

                Case "1"
                    Dim DB As New Acceso(Me.txtBaseDatos.Text, Me.txtServidor.Text, Me.txtusuario.Text, Me.txtPassword.Text, Me.txtBaseDatosDestino.Text)
                    txtSQL.Text = ""
                    Dim dr As DataRow = Nothing
                    Dim itm As System.Windows.Forms.ListViewItem

                    For Each itm In lstTablas.CheckedItems
                        txtSQL.Text &= DB.Get_Script(itm.Text, Me.chkAlta.Checked, Me.chkBaja.Checked, Me.chkModificacion.Checked)
                    Next
                    TabMain.SelectedIndex = 2

            End Select



        Catch er As Exception
            MsgBox(Err.Description, vbExclamation)
            TabMain.SelectedIndex = 0
        End Try

    End Sub

    Private Sub btnCerrar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCerrar.Click
        Me.Close()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If txtSQL.Text <> "" Then Clipboard.SetText(txtSQL.Text)
    End Sub

    Private Sub TabPage1_Click(sender As Object, e As EventArgs) Handles TabPage1.Click

    End Sub
End Class


Public Class Combo_item

    Private m_id As Int16
    Private m_description As String

    Sub New(p_id As Int16, p_descripcion As String)
        m_id = p_id
        m_description = p_descripcion
    End Sub

    Public Property Descripcion() As String
        Get
            Return m_description
        End Get
        Set(value As String)
            m_description = value
        End Set
    End Property

    Public Property id() As Int16
        Get
            Return m_id
        End Get
        Set(value As Int16)
            m_id = value
        End Set
    End Property

End Class
