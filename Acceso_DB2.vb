Imports IBM.Data.DB2.iSeries

Public Class Acceso_DB2
    Implements IDisposable

    Private strCon As String

    Private Con As iDB2Connection
    Private cmd As iDB2Command

    Protected strSQL, m_ServerName, m_usuario, m_pass, m_DataBaseName, m_DataBaseName_Destino As String


    Sub New(ByVal DataBaseName As String, ByVal ServerName As String, ByVal user As String, ByVal pass As String, ByVal DataBaseName_Destino As String)

        Try
            strCon = "Datasource=" & ServerName & ";UserID = " & user & "; Password = " & pass & ";"
            Con = New iDB2Connection(strCon)
            Con.Open()
            cmd = New iDB2Command
            cmd.Connection = Con
            cmd.CommandTimeout = 120

            Me.m_pass = pass
            Me.m_ServerName = ServerName
            Me.m_usuario = user
            Me.m_DataBaseName = DataBaseName
            Me.m_DataBaseName_Destino = DataBaseName_Destino

        Catch ex As SqlClient.SqlException
            Throw ex

        Catch ex2 As Exception
            Throw ex2

        End Try

    End Sub

    Public Function ExecuteSQL(ByVal strSQL As String) As Boolean

        Try
            cmd.CommandText = strSQL
            cmd.ExecuteNonQuery()
            Return True

        Catch e As Exception
            strSQL = ""
            Throw e
            Return False
        Finally

        End Try

    End Function

    Public Function EjecutarConsulta(ByVal strSQL As String) As DataTable

        Dim DA As iDB2DataAdapter
        Dim DT As DataTable

        Try
            cmd.CommandText = strSQL
            DA = New iDB2DataAdapter(cmd)
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

        Dim DA As iDB2DataAdapter
        Dim DT As DataTable

        Try
            cmd.CommandText = strSQL
            DA = New iDB2DataAdapter(cmd)
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
            Return Convert.ToDateTime(EjecutarEscalar(" select current_date"))

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
        dr = Me.EjecutarFila("Select TABLE_NAME from SYSIBM.TABLES WHERE TABLE_SCHEMA='" & Me.m_DataBaseName_Destino & "' AND TABLE_NAME='AUDIT_" & tabla & "'")
        dtCampos = Me.EjecutarConsulta("Select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUN_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, IS_NULLABLE, CHARACTER_SET_NAME FROM SYSIBM.COLUMNS WHERE TABLE_SCHEMA='" & Me.m_DataBaseName & "' AND TABLE_NAME='" & tabla & "' ORDER BY ORDINAL_POSITION")

        If dr Is Nothing Then

            strSQL = strSQL & vbCrLf
            'strSQL = strSQL & "DROP TABLE  " & Me.m_DataBaseName_Destino & ".AUDIT_" & itm.Text & vbCrLf
            'strSQL = strSQL & vbCrLf

            strSQL = strSQL & "CREATE TABLE " & Me.m_DataBaseName_Destino & ".AUDIT_" & tabla & vbCrLf
            strSQL = strSQL & "(" & vbCrLf
            strSQL = strSQL & " AUDIT_ID INTEGER GENERATED ALWAYS AS IDENTITY ," & vbCrLf
            strSQL = strSQL & " AUDIT_FECHA DATE DEFAULT CURRENT_DATE," & vbCrLf
            strSQL = strSQL & " AUDIT_USUARIO VARCHAR(50) DEFAULT CURRENT_USER," & vbCrLf
            strSQL = strSQL & " AUDIT_ACCION CHAR(2) CCSID 284 NOT NULL ," & vbCrLf

            Dim c As Integer = 0
            For Each campo In dtCampos.Rows
                c = c + 1
                strSQL = strSQL & campo("COLUMN_NAME").ToString & " " & campo("DATA_TYPE").ToString

                Select Case Convert.ToInt32(campo("DATA_TYPE").ToString)
                    Case "INTEGER", "BIGINT"
                        strSQL = strSQL & " INTEGER "
                    Case "CHARACTER"
                        strSQL = strSQL & "(" & campo("CHARACTER_MAXIMUN_LENGTH").ToString & ")  CCSID " & campo("CHARACTER_SET_NAME").ToString().Replace("IBM", "")
                    Case "NUMERIC"
                        strSQL = strSQL & "(" & campo("NUMERIC_PRECISION").ToString & "," & campo("NUMERIC_SCALE").ToString & ") "
                End Select

                If (campo("IS_NULLABLE").ToString <> "YES") Then
                    strSQL = strSQL & " NOT NULL" & vbCrLf
                End If

                If (c < dtCampos.Rows.Count) Then
                    strSQL = strSQL & ", "
                End If

            Next

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