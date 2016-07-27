
Public Class Acceso
    Implements IDisposable

    Private strCon As String

    Private Con As System.Data.SqlClient.SqlConnection
    Private cmd As System.Data.SqlClient.SqlCommand

    Protected strSQL, m_DataBaseName, m_DataBaseName_Destino, m_ServerName, m_usuario, m_pass As String



    Sub New(ByVal DataBaseName As String, ByVal ServerName As String, ByVal usuario As String, ByVal pass As String, ByVal DataBaseName_Destino As String)

        Try
            strCon = "user id = " & usuario & "; password = " & pass & ";Initial Catalog=" & DataBaseName & ";Data Source=" & ServerName & ";"
            Con = New System.Data.SqlClient.SqlConnection(strCon)
            Con.Open()
            cmd = New System.Data.SqlClient.SqlCommand
            cmd.Connection = Con
            cmd.CommandTimeout = 120
            cmd.CommandText = "set dateformat 'YMD'"
            cmd.ExecuteNonQuery()

            Me.m_DataBaseName = DataBaseName
            Me.m_DataBaseName_Destino = DataBaseName_Destino
            Me.m_pass = pass
            Me.m_ServerName = ServerName
            Me.m_usuario = usuario

        Catch ex As SqlClient.SqlException
            Throw ex

        Catch ex2 As Exception
            Throw ex2

        End Try

    End Sub

    Public Function ExecuteSQL(ByVal strSQL As String) As Boolean

        Try
            strSQL = " set dateformat 'YMD';" & strSQL
            cmd.CommandText = strSQL
            cmd.ExecuteNonQuery()
            strSQL = ""
            Return True

        Catch e As Exception
            strSQL = ""
            Throw e
            Return False
        Finally

        End Try

    End Function

    Public Function EjecutarConsulta(ByVal strSQL As String) As DataTable

        Dim DA As System.Data.SqlClient.SqlDataAdapter
        Dim DT As DataTable

        Try
            strSQL = " set dateformat 'YMD';" & strSQL
            cmd.CommandText = strSQL
            DA = New SqlClient.SqlDataAdapter(cmd)
            DT = New DataTable
            DA.Fill(DT)
            Return DT
        Catch e As Exception
            Throw e
            Return Nothing
        Finally
            DA = Nothing
            DT = Nothing
            strSQL = ""
        End Try

    End Function


    Public Function EjecutarFila(ByVal strSQL As String) As DataRow

        Dim DA As System.Data.SqlClient.SqlDataAdapter
        Dim DT As DataTable

        Try
            strSQL = " set dateformat 'YMD';" & strSQL
            cmd.CommandText = strSQL
            DA = New SqlClient.SqlDataAdapter(cmd)
            DT = New DataTable
            DA.Fill(DT)

            If DT.Rows.Count > 0 Then
                Return DT.Rows(0)
            Else
                Return Nothing
            End If
        Catch e As Exception
            Throw e
            Return Nothing
        Finally
            DA = Nothing
            DT = Nothing
            strSQL = ""
        End Try

    End Function

    Public Function EjecutarEscalar(ByVal strSQL As String) As Object

        Try
            cmd.CommandText = strSQL
            Return cmd.ExecuteScalar()

        Catch e As Exception

            Throw e
            Return Nothing
        Finally
            strSQL = ""
        End Try

    End Function

    Public Function FechaServidor() As DateTime

        Try
            Return Convert.ToDateTime(EjecutarEscalar(" select getdate()"))

        Catch e As Exception
            Throw e
            Return Nothing

        End Try

    End Function

    Public Function NZ(ByVal Valor As Object, ByVal ValorSiNulo As String) As String
        Try
            If IsDBNull(Valor) Then
                Return ValorSiNulo
            Else
                Return Valor.ToString
            End If

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Public Sub Dispose() Implements System.IDisposable.Dispose
        Try
            If Con.State = 1 Then
                Con.Close()
            End If
            Con = Nothing
            cmd = Nothing
        Catch e As Exception
            Throw e

        Finally
            strSQL = ""
        End Try

    End Sub


    Public Function Get_Script(ByVal tabla As String, alta As Boolean, baja As Boolean, modificacion As Boolean) As String

        strSQL = ""
        Dim dr As DataRow = Nothing


            Dim dtCampos As DataTable
            Dim campo As DataRow

            'VERIFICAR QUE LA TABLA NO EXISTA
            dr = Me.EjecutarFila("Select * from " & Me.m_DataBaseName_Destino & ".dbo.SysObjects where name='audit" & tabla & "'")
            dtCampos = Me.EjecutarConsulta("Select * from Syscolumns where id=" & tabla & " order by name")

            If dr Is Nothing Then
                strSQL &= "USE " & Me.m_DataBaseName_Destino & vbCrLf
                strSQL = strSQL & "GO" & vbCrLf

                strSQL = strSQL & vbCrLf
                'strSQL = strSQL & "DROP TABLE [audit" & itm.Text & "] " & vbCrLf
                'strSQL = strSQL & "GO" & vbCrLf
                'strSQL = strSQL & vbCrLf

                strSQL = strSQL & "CREATE TABLE [audit" & tabla & "] " & vbCrLf
                strSQL = strSQL & "(" & vbCrLf
                strSQL = strSQL & "[auditId] [int] IDENTITY (1, 1) NOT NULL ," & vbCrLf
                strSQL = strSQL & "[auditFechaHora] [datetime] NULL CONSTRAINT [DF_audit" & tabla & "_auditFechaHora] DEFAULT (getdate())," & vbCrLf
                strSQL = strSQL & "[auditAccion] [varchar] (2) NULL ," & vbCrLf
                'strSQL = strSQL & "[auditIdUsuario] [int] NULL ," & vbCrLf
                strSQL = strSQL & "[auditUsuario] [varchar] (50) NULL CONSTRAINT [DF_audit" & tabla & "_auditUsuario] DEFAULT (user_name())," & vbCrLf
                strSQL = strSQL & "[auditAplicacion] [varchar] (100) NULL CONSTRAINT [DF_audit" & tabla & "_auditAplicacion] DEFAULT (left(app_name(),100))," & vbCrLf
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "]"
                    Select Case Convert.ToInt32(campo("xtype").ToString)
                        Case 56
                            strSQL = strSQL & " [int] "
                        Case 127
                            strSQL = strSQL & " [bigint] "
                        Case 52
                            strSQL = strSQL & " [smallint] "
                        Case 48
                            strSQL = strSQL & " [tinyint] "
                        Case 61
                            strSQL = strSQL & " [datetime] "
                        Case 58
                            strSQL = strSQL & " [smalldatetime] "
                        Case 60
                            strSQL = strSQL & " [money] "
                        Case 104
                            strSQL = strSQL & " [bit] "
                        Case 106
                            strSQL = strSQL & " [decimal] (" & campo("xprec").ToString & "," & campo("xscale").ToString & ")"
                        Case 62
                            strSQL = strSQL & " [float] "
                        Case 108
                            strSQL = strSQL & " [numeric] (" & campo("xprec").ToString & "," & campo("xscale").ToString & ")"
                        Case 167
                            strSQL = strSQL & " [varchar] (" & campo("length").ToString & ")"
                        Case 175
                            strSQL = strSQL & " [char] (" & campo("length").ToString & ")"
                        Case 239
                            strSQL = strSQL & " [nchar] (" & campo("length").ToString & ")"
                        Case 231
                            strSQL = strSQL & " [nvarchar] (" & campo("length").ToString & ")"
                        Case Else
                            Err.Raise(65000, "cmdSiguiente2_Click", "Tipo de Dato no soportado por El Auditor: " & campo("name").ToString)
                    End Select
                    strSQL = strSQL & " NULL," & vbCrLf
                Next

                strSQL = strSQL & " CONSTRAINT [PK_audit" & tabla & "] PRIMARY KEY CLUSTERED" & vbCrLf
                strSQL = strSQL & " (" & vbCrLf
                strSQL = strSQL & " [auditId]" & vbCrLf
                strSQL = strSQL & " )  ON [PRIMARY]" & vbCrLf
                strSQL = strSQL & ")" & vbCrLf
                strSQL = strSQL & "ON [PRIMARY]" & vbCrLf
                strSQL = strSQL & "GO" & vbCrLf
                strSQL = strSQL & vbCrLf

            End If

            strSQL = strSQL & "USE " & Me.m_DataBaseName & vbCrLf
            strSQL = strSQL & "GO" & vbCrLf
            strSQL = strSQL & vbCrLf

            If alta Then

                dr = Me.EjecutarFila("Select * from " & Me.m_DataBaseName & ".dbo.SysObjects where name='trg" & itm.Text & "Alta'")

                If Not dr Is Nothing Then
                    strSQL = strSQL & "DROP TRIGGER trg" & tabla & "Alta " & vbCrLf
                    strSQL = strSQL & "GO" & vbCrLf
                    strSQL = strSQL & vbCrLf
                End If

                strSQL = strSQL & "CREATE TRIGGER trg" & tabla & "Alta ON [dbo].[" & tabla & "]" & vbCrLf
                strSQL = strSQL & "FOR INSERT" & vbCrLf
                strSQL = strSQL & "AS" & vbCrLf
                strSQL = strSQL & "SET NOCOUNT ON" & vbCrLf
                'strSQL = strSQL & "DECLARE @IdUsuario int" & vbCrLf
                'strSQL = strSQL & "SELECT @IdUsuario=IdUsuario FROM TbSpids WHERE Spid=@@Spid" & vbCrLf
                strSQL = strSQL & "INSERT INTO " & Me.m_DataBaseName_Destino & ".dbo.audit" & tabla & vbCrLf
                strSQL = strSQL & "("
                strSQL = strSQL & "auditAccion, "
                'strSQL = strSQL & "auditIdUsuario, "
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "], "
                Next
                strSQL = strSQL.Substring(0, strSQL.Length - 2)
                strSQL = strSQL & ")" & vbCrLf
                strSQL = strSQL & "SELECT "
                strSQL = strSQL & "'A',"
                'strSQL = strSQL & "@IdUsuario,"
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "], "
                Next
                strSQL = strSQL.Substring(0, strSQL.Length - 2)
                strSQL = strSQL & " FROM Inserted" & vbCrLf
                strSQL = strSQL & "GO" & vbCrLf
                strSQL = strSQL & vbCrLf
            End If

            If baja Then

                dr = Me.EjecutarFila("Select * from " & Me.m_DataBaseName & ".dbo.SysObjects where name='trg" & tabla & "Baja'")

                If Not dr Is Nothing Then
                    strSQL = strSQL & "DROP TRIGGER trg" & tabla & "Baja " & vbCrLf
                    strSQL = strSQL & "GO" & vbCrLf
                    strSQL = strSQL & vbCrLf
                End If

                strSQL = strSQL & "CREATE TRIGGER trg" & tabla & "Baja ON [dbo].[" & tabla & "]" & vbCrLf
                strSQL = strSQL & "FOR DELETE" & vbCrLf
                strSQL = strSQL & "AS" & vbCrLf
                strSQL = strSQL & "SET NOCOUNT ON" & vbCrLf
                'strSQL = strSQL & "DECLARE @IdUsuario int" & vbCrLf
                'strSQL = strSQL & "SELECT @IdUsuario=IdUsuario FROM TbSpids WHERE Spid=@@Spid" & vbCrLf
                strSQL = strSQL & "INSERT INTO " & Me.m_DataBaseName_Destino & ".dbo.audit" & tabla & vbCrLf
                strSQL = strSQL & "("
                strSQL = strSQL & "auditAccion, "
                'strSQL = strSQL & "auditIdUsuario, "
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "], "
                Next
                strSQL = strSQL.Substring(0, strSQL.Length - 2)
                strSQL = strSQL & ")" & vbCrLf
                strSQL = strSQL & "SELECT "
                strSQL = strSQL & "'B',"
                'strSQL = strSQL & "@IdUsuario,"
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "], "
                Next
                strSQL = strSQL.Substring(0, strSQL.Length - 2)
                strSQL = strSQL & " FROM Deleted" & vbCrLf
                strSQL = strSQL & "GO" & vbCrLf
                strSQL = strSQL & vbCrLf
            End If


            If modificacion Then
                dr = Me.EjecutarFila("Select * from " & Me.m_DataBaseName & ".dbo.SysObjects where name='trg" & tabla & "Modi'")

                If Not dr Is Nothing Then
                    strSQL = strSQL & "DROP TRIGGER trg" & tabla & "Modi " & vbCrLf
                    strSQL = strSQL & "GO" & vbCrLf
                    strSQL = strSQL & vbCrLf
                End If

                strSQL = strSQL & "CREATE TRIGGER trg" & tabla & "Modi ON [dbo].[" & tabla & "]" & vbCrLf
                strSQL = strSQL & "FOR UPDATE" & vbCrLf
                strSQL = strSQL & "AS" & vbCrLf
                strSQL = strSQL & "SET NOCOUNT ON" & vbCrLf
                'strSQL = strSQL & "DECLARE @IdUsuario int" & vbCrLf
                'strSQL = strSQL & "SELECT @IdUsuario=IdUsuario FROM TbSpids WHERE Spid=@@Spid" & vbCrLf
                strSQL = strSQL & "INSERT INTO " & Me.m_DataBaseName_Destino & ".dbo.audit" & tabla & vbCrLf
                strSQL = strSQL & "("
                strSQL = strSQL & "auditAccion, "
                'strSQL = strSQL & "auditIdUsuario, "
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "], "
                Next
                strSQL = strSQL.Substring(0, strSQL.Length - 2)
                strSQL = strSQL & ")" & vbCrLf
                strSQL = strSQL & "SELECT "
                strSQL = strSQL & "'MB',"
                'strSQL = strSQL & "@IdUsuario,"
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "], "
                Next
                strSQL = strSQL.Substring(0, strSQL.Length - 2)
                strSQL = strSQL & " FROM Deleted " & vbCrLf
                strSQL = strSQL & "INSERT INTO " & Me.m_DataBaseName_Destino & ".dbo.audit" & tabla & vbCrLf
                strSQL = strSQL & "("
                strSQL = strSQL & "auditAccion, "
                'strSQL = strSQL & "auditIdUsuario, "
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "], "
                Next
                strSQL = strSQL.Substring(0, strSQL.Length - 2)
                strSQL = strSQL & ")" & vbCrLf
                strSQL = strSQL & "SELECT "
                strSQL = strSQL & "'MA',"
                'strSQL = strSQL & "@IdUsuario,"
                For Each campo In dtCampos.Rows
                    strSQL = strSQL & "[" & campo("name").ToString & "], "
                Next
                strSQL = strSQL.Substring(0, strSQL.Length - 2)
                strSQL = strSQL & " FROM Inserted" & vbCrLf
                strSQL = strSQL & "GO" & vbCrLf
                strSQL = strSQL & vbCrLf
            End If

        Return strSQL

    End Function


End Class