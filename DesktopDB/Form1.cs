using System;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DesktopDB
{
    public partial class Form1 : Form
    {
        private DataSet dataSet;
        private NpgsqlConnection connection;
        private List<User> users = new List<User>();

        struct User
        {
            public int id;
            public String login;
            public String password;
            public int fk_address;
            public String role;

            public User(string login, string password, int fk_address, string role)
            {
                this.id = -1;
                this.login = login;
                this.password = password;
                this.fk_address = fk_address;
                this.role = role;
            }

            public User(int id, string login, string password, int fk_address, string role)
            {
                this.id = id;
                this.login = login;
                this.password = password;
                this.fk_address = fk_address;
                this.role = role;
            }

            public String ToString()
            {
                return $"{id} {login} {password} {fk_address} {role}";
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connection = new NpgsqlConnection("server=localhost;Port=5432;Database=restaurant;" +
                "Username=postgres;Password=6010");
            connection.Open();

            ShowUsers();

            GetUsersFromTable();

            dataGridView1.CellValueChanged += changeCell;
            dataGridView1.UserDeletingRow += deleteRow;
        }

        protected override void OnClosed(EventArgs e)
        {
            Console.WriteLine("End program");
            connection.Close();
            base.OnClosed(e);
        }

        private void changeCell(object sender, EventArgs e)
        {
            if (!(dataGridView1.CurrentRow.Cells[1].Value is DBNull) &&
                !(dataGridView1.CurrentRow.Cells[2].Value is DBNull) &&
                !(dataGridView1.CurrentRow.Cells[3].Value is DBNull) &&
                !(dataGridView1.CurrentRow.Cells[4].Value is DBNull))
            {
                if (!(dataGridView1.CurrentRow.Cells[0].Value is DBNull)) {
                    EditUser(new User((int)dataGridView1.CurrentRow.Cells[0].Value,
                                      (string)dataGridView1.CurrentRow.Cells[1].Value,
                                      (string)dataGridView1.CurrentRow.Cells[2].Value,
                                      (int)dataGridView1.CurrentRow.Cells[3].Value,
                                      (string)dataGridView1.CurrentRow.Cells[4].Value));
                    Console.WriteLine($"Changed: {dataGridView1.CurrentCell.Value}");
                }
                else
                {
                    CreateUser(new User((string)dataGridView1.CurrentRow.Cells[1].Value,
                                        (string)dataGridView1.CurrentRow.Cells[2].Value,
                                        (int)dataGridView1.CurrentRow.Cells[3].Value,
                                        (string)dataGridView1.CurrentRow.Cells[4].Value));
                    Console.WriteLine($"Created");
                }
            }
        }

        private bool ExistUser(int id)
        {
            NpgsqlCommand getCommand = new NpgsqlCommand($"select * from clients where id = {id}", connection);
            List<User> result = new List<User>();
            using (NpgsqlDataReader reader = getCommand.ExecuteReader())
            {
                return reader.HasRows;
            }
        }

        private void CreateUser(User newUser)
        {
            NpgsqlCommand addCommand;
            if (newUser.fk_address < 0)
            {
                addCommand = new NpgsqlCommand($"INSERT INTO clients (email, psswrd, role) " +
                                                             $"VALUES ('{newUser.login}', '{newUser.password}', '{newUser.role}')",
                                                        connection);
            }
            else
            {
                addCommand = new NpgsqlCommand($"INSERT INTO clients (email, psswrd, fk_address, role) " +
                                                             $"VALUES ('{newUser.login}', '{newUser.password}', {newUser.fk_address}, '{newUser.role}')",
                                                        connection);
            }
            addCommand.ExecuteNonQuery();
            ShowUsers();
        }

        private void EditUser(User updatedUser)
        {
            Console.WriteLine(updatedUser.ToString());
            NpgsqlCommand updateCommand;
            if (updatedUser.fk_address < 0)
            {
                updateCommand = new NpgsqlCommand($"update clients " +
                                                                $"set email = '{updatedUser.login}', " +
                                                                $"psswrd = '{updatedUser.password}', " +
                                                                $"fk_address = {null}, " +
                                                                $"role = '{updatedUser.role}' " +
                                                                $"where id = {updatedUser.id}",
                                                                connection);
            }
            else
            {
                updateCommand = new NpgsqlCommand($"update clients " +
                                                                $"set email = '{updatedUser.login}', " +
                                                                $"psswrd = '{updatedUser.password}', " +
                                                                $"fk_address = {updatedUser.fk_address}, " +
                                                                $"role = '{updatedUser.role}' " +
                                                                $"where id = {updatedUser.id}",
                                                                connection);
            }

            updateCommand.ExecuteNonQuery();
            ShowUsers();
        }

        private void deleteRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DeleteUser((int) dataGridView1.CurrentRow.Cells[0].Value);
            //ShowUsers();
            Console.WriteLine($"Deleted: {dataGridView1.CurrentRow.Index}");
        }

        private void DeleteUser(int id)
        {
            NpgsqlCommand deleteCommand = new NpgsqlCommand($"delete from clients where id = {id}", connection);
            deleteCommand.ExecuteNonQuery();
        }

        private void GetUsersFromTable()
        {
            List<User> result = new List<User>();
            for(int row = 0; row < dataGridView1.RowCount - 1; row++)
            {
                result.Add(new User(Convert.ToInt32(dataGridView1[0, row].Value.ToString()), 
                                    dataGridView1[1, row].Value.ToString(),
                                    dataGridView1[2, row].Value.ToString(),
                                    Convert.ToInt32(dataGridView1[3, row].Value.ToString()),
                                    dataGridView1[4, row].Value.ToString()));
                Console.WriteLine(result[result.Count-1].ToString());
            }
        }

        private void ShowUsers()
        {
            users = GetUsers();

            DataTable table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("login", typeof(string));
            table.Columns.Add("password", typeof(string));
            table.Columns.Add("fk_address", typeof(int));
            table.Columns.Add("role", typeof(string));

            foreach (User user in users)
            {
                table.Rows.Add(user.id, user.login, user.password, user.fk_address, user.role);
            }

            dataGridView1.DataSource = table;
        }

        private List<User> GetUsers()
        {
            NpgsqlCommand getCommand = new NpgsqlCommand("Select * from clients", connection);
            List<User> result = new List<User>();
            using (NpgsqlDataReader reader = getCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    int fk_address = !(reader[3] is DBNull) ? (int) reader[3] : -1;
                    //Console.WriteLine(reader[3].GetType() + " " + (!(reader[3] is DBNull) ? "int" : "dbnulls"));
                    result.Add(new User((int)reader[0],
                                        (String)reader[1],
                                        (String)reader[2],
                                        fk_address,
                                        (String)reader[4]));
                }
            }
            return result;
        }
    }
}
