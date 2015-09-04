using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessesPlanning
{
    public partial class Form1 : Form
    {
        private int _quantum;
        public BindingList<Process> _processes;

        public Form1()
        {
            InitializeData();
            InitializeComponent();

            bindingSourceProcesses.DataSource = _processes;
            dataGridViewProcesses.AutoGenerateColumns = true;
            dataGridViewProcesses.DataSource = bindingSourceProcesses;
            bindingNavigatorProcesses.BindingSource = bindingSourceProcesses;
            numericUpDownQuantum.Value = _quantum;
        }

        private void InitializeData()
        {
            _quantum = 2;
            _processes = CreateNewProcessBindingList();
        }

        private BindingList<Process> CreateNewProcessBindingList()
        {
            return new BindingList<Process>()
            {
                new Process(1, 0, 8),
                new Process(2, 2, 5),
                new Process(3, 4, 1),
                new Process(4, 5, 2)
            }; 
        }

        private IOrderedEnumerable<Process> CreateCopyProcessIOrderedEnumerable(BindingList<Process> list)
        {
            BindingList<Process> result = new BindingList<Process>();
            foreach (Process process in list)
            {
                Process current = new Process(process.Id, process.ArrivalTime, process.ExecutionTime);
                result.Add(current);
            }
            return result.OrderBy(p => p.ArrivalTime);
        }


        private void dataGridViewProcesses_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) // allow user to enter only integers
        {
            string sCellName =  dataGridViewProcesses.Columns[dataGridViewProcesses.CurrentCell.ColumnIndex].Name;
            if (dataGridViewProcesses.CurrentCell.Value == DBNull.Value)
            {
                dataGridViewProcesses.CurrentCell.Value = 0;
            }
            if (sCellName.ToUpper() == "ID" ||
                sCellName.ToUpper() == "ARRIVALTIME" ||
                sCellName.ToUpper() == "EXECUTIONTIME") 
            {
                e.Control.KeyPress += new KeyPressEventHandler(CheckKey);
            }
        }

        private void CheckKey(object sender, KeyPressEventArgs e) 
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private List<Result> CreateResultList(BindingList<Process> processes)
        {
            List<Result> results = new List<Result>();
            foreach (Process process in processes)
            {
                results.Add(new Result() { ProcessId = process.Id });
            }
            return results;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {

            dataGridViewProcesses.Refresh();

            ObservableCollection<Process> sortedProcesses = new ObservableCollection<Process>(CreateCopyProcessIOrderedEnumerable(_processes));
            int firstReadyProcessIndex = -1;
            int lastReadyProcessIndex = -1;
            int realExecutionTime = 0; // from 1 to quantum
            int currentTime = 0;

            List<Result> results = CreateResultList(_processes);

            while (sortedProcesses.Count > 0)
            {
                if (sortedProcesses[0].ArrivalTime <= 0) // if process is ready
                {
                    if (sortedProcesses[0].ExecutionTime > 0) // and its expected execution time is larger than quantum
                    {
                        if (sortedProcesses[0].ExecutionTime > _quantum)
                        {
                            realExecutionTime = _quantum;
                            currentTime += realExecutionTime;
                            firstReadyProcessIndex = 0;
                        }
                        else // task comleted
                        {
                            realExecutionTime = sortedProcesses[0].ExecutionTime; // do the rest of work
                            currentTime += realExecutionTime;
                            if (results.Exists(result => result.ProcessId == sortedProcesses[0].Id))
                            {
                                results.First(result => result.ProcessId == sortedProcesses[0].Id).EndTime = currentTime;
                            }
                            firstReadyProcessIndex = -1; // default
                        }                       
                        sortedProcesses[0].ExecutionTime -= realExecutionTime; // do work
                        Debug.Write("P" + sortedProcesses[0].Id.ToString() + " " + sortedProcesses[0].ExecutionTime.ToString()); // prints remaining time
                        Debug.WriteLine(" " + realExecutionTime.ToString()); // prints real execution time

                        for (int i = 1; i < sortedProcesses.Count; i++) // i != 0 cause if i = 0 then this process just executed 
                        {
                            //
                            // calculate pause time for results
                            // 
                            if (results.Exists(result => result.ProcessId == sortedProcesses[i].Id) &&
                                sortedProcesses[i].ArrivalTime <= 0)
                            {
                                if (sortedProcesses[i].Id == 4)
                                    Debug.WriteLine("Added " + realExecutionTime);
                                results.First(result => result.ProcessId == sortedProcesses[i].Id).PauseTime += realExecutionTime;
                            }
                        }

                        if (sortedProcesses[0].ExecutionTime <= 0)
                            sortedProcesses.RemoveAt(0); // remove completed task
                        
                    }
                    else // task is completed (never should enter this case)
                    {
                        sortedProcesses.RemoveAt(0); // remove completed task
                        firstReadyProcessIndex = -1; // default
                    }
                }


                for (int i = 0; i < sortedProcesses.Count; i++)
                {
                    // handle arrival time in remaining processes
                    Process process = sortedProcesses[i];
                    process.ArrivalTime -= realExecutionTime;
                    if (process.ArrivalTime <= 0)
                    {
                        process.ArrivalTime = 0;
                        lastReadyProcessIndex = i;
                    }               
                }

                if (firstReadyProcessIndex == 0 && lastReadyProcessIndex > 0) 
                {
                    // move currently executing process to the end of READY processes
                    sortedProcesses.Move(firstReadyProcessIndex, lastReadyProcessIndex);
                }
            }

            if (results != null)
            {
                bindingSourceResults.DataSource = results;
                dataGridViewResults.DataSource = bindingSourceResults;
            }

        }

        private void numericUpDownQuantum_ValueChanged(object sender, EventArgs e)
        {
            _quantum = (int)numericUpDownQuantum.Value;
        } 
    }
}
