using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public class DAL
    {
        private static string connectionString ="Data Source=KAIDEN\\BLAZE;Initial Catalog=DBAkademikADO;User ID=sa;Password=towinnadzul09122005;";

        public string GetConnectionString()
        {
            return connectionString;
        }

        // Sekarang bisa pakai connectionString langsung
        SqlConnection conn = new SqlConnection(connectionString);

        SqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi;

        // ======= COUNT =======
        public int CountMhs()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
            outputParam.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();
            return Convert.ToInt32(outputParam.Value);
        }

        public static string GetLoacalIPAddress()
        {
            string localIP = string.Empty;
            try
            {
                var host = System.Net.Dns.GetHostEntry(
                           System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily ==
                        System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting local IP address: " + ex.Message);
            }
            return localIP;
        }

        // ======= READ =======
        public DataTable GetMhs()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            da = new SqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);

            return dtMahasiswa;
        }

        public DataTable GetMhsByNIM(string nim)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_GetMahasiswaByNIM", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("pNIM", nim);

            da = new SqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);

            return dtMahasiswa;
        }

        // ======= INSERT (dengan foto) =======
        public void InsertMhs(string nim, string nama, string alamat,
                              string jenisKelamin, DateTime tanggalLahir,
                              string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                SqlCommand command = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@NIM", nim);
                command.Parameters.AddWithValue("@Nama", nama);
                command.Parameters.AddWithValue("@Alamat", alamat);
                command.Parameters.AddWithValue("@JenisKelamin", jenisKelamin);
                command.Parameters.AddWithValue("@TanggalLahir", tanggalLahir);
                command.Parameters.AddWithValue("@KodeProdi", kodeProdi);
                command.Parameters.AddWithValue("@TanggalDaftar", DateTime.Now);
                // Parameter foto — jika null kirim DBNull
                command.Parameters.AddWithValue("@pFoto",
                    (object)foto ?? DBNull.Value);

                command.ExecuteNonQuery();

                // Log aktivitas (sesuai tabel LogAktivitas Anda)
                SqlCommand cmdLog = new SqlCommand(
                    "INSERT INTO LogAktivitas (aktivitas, waktu) VALUES (@aktivitas, GETDATE())",
                    conn, trans);
                cmdLog.Parameters.AddWithValue("@aktivitas",
                    "INSERT MANUAL VIA APP - NIM: " + nim);
                cmdLog.ExecuteNonQuery();

                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw; // lempar ke pemanggil agar bisa ditangkap di Form
            }
            finally
            {
                conn.Close();
            }
        }

        // ======= UPDATE (dengan foto) =======
        public void UpdateMhs(string nim, string nama, string alamat,
                              string jenisKelamin, DateTime tanggalLahir,
                              string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand command = new SqlCommand("sp_UpdateMahasiswa", conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@NIM", nim);
            command.Parameters.AddWithValue("@Nama", nama);
            command.Parameters.AddWithValue("@Alamat", alamat);
            command.Parameters.AddWithValue("@JenisKelamin", jenisKelamin);
            command.Parameters.AddWithValue("@TanggalLahir", tanggalLahir);
            command.Parameters.AddWithValue("@KodeProdi", kodeProdi);
            command.Parameters.AddWithValue("@pFoto",
                (object)foto ?? DBNull.Value);

            command.ExecuteNonQuery();
        }

        // ======= DELETE =======
        public void DeleteMhs(string nim)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);
            cmd.Parameters.Add("@NIM", SqlDbType.Char, 11).Value = nim;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.ExecuteNonQuery();
        }

        // ======= RESET DATA =======
        public void resetData()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            string query = @"
                IF OBJECT_ID('dbo.Mahasiswa_Backup') IS NOT NULL
                BEGIN
                    DELETE FROM dbo.Mahasiswa;
                    INSERT INTO dbo.Mahasiswa
                    SELECT * FROM dbo.Mahasiswa_Backup;
                END";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();
        }

        // ======= TEST INJECT =======
        public void testInject(string nim)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            string query = "Update mahasiswa set nama = 'HACKED' where NIM = " + nim;
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();
        }

        // ======= LOG =======
        public void InsertLog(string message)
        {
            using (SqlConnection logConn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO LogError (waktu, pesan_error) VALUES (GETDATE(), @pesan)";
                using (SqlCommand cmd = new SqlCommand(query, logConn))
                {
                    cmd.Parameters.AddWithValue("@pesan", message);
                    logConn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ======= PRODI =======
        public DataTable getProdi()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("select namaprodi from prodi", conn);
            cmd.CommandType = CommandType.Text;
            dtProdi = new DataTable();
            da = new SqlDataAdapter(cmd);
            da.Fill(dtProdi);

            return dtProdi;
        }

        public DataTable getDataRekap(string prodi, DateTime tanggalMasuk)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_Report", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@inProdi", prodi);
            cmd.Parameters.AddWithValue("@inTglMsuk", tanggalMasuk.Year.ToString());

            da = new SqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);
            return dtMahasiswa;
        }

        // ======= CHART =======
        public DataTable getAllDataChart()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_DashBoard", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            da = new SqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);
            return dtMahasiswa;
        }

        public DataTable getDataChartByTahun(DateTime thMasuk)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_DashBoardByTahun", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@inTglMsuk", thMasuk.Year);
            da = new SqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);
            return dtMahasiswa;
        }
    }
}