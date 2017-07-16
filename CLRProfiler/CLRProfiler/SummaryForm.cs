using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for SummaryForm.
	/// </summary>
    public partial class SummaryForm : System.Windows.Forms.Form
    {

        private readonly ReadNewLog log;
        private readonly ReadLogResult logResult;
        private readonly string scenario = "";

        private string FormatNumber(double number)
        {
            return string.Format("{0:n0}", number);
        }

        private string CalculateTotalSize(Histogram histogram)
        {
            double totalSize = 0.0;
            for (int i = 0; i < histogram.typeSizeStacktraceToCount.Length; i++)
            {
                int count = histogram.typeSizeStacktraceToCount[i];
                if (count > 0)
                {
                    int[] stacktrace = histogram.readNewLog.stacktraceTable.IndexToStacktrace(i);
                    int size = stacktrace[1];
                    totalSize += (ulong)size*(ulong)count;
                }
            }
            return FormatNumber(totalSize);
        }

        private string CalculateTotalCount(Histogram histogram)
        {
            double totalCount = 0.0;
            for (int i = 0; i < histogram.typeSizeStacktraceToCount.Length; i++)
            {
                int count = histogram.typeSizeStacktraceToCount[i];
                totalCount += count;
            }
            return FormatNumber(totalCount);
        }

        private Histogram GetFinalHeapHistogram()
        {
            Histogram histogram = new Histogram(log);
            LiveObjectTable.LiveObject o;
            for (logResult.liveObjectTable.GetNextObject(0, ulong.MaxValue, out o);
                o.id < ulong.MaxValue && o.id + o.size >= o.id;
                logResult.liveObjectTable.GetNextObject(o.id + o.size, ulong.MaxValue, out o))
            {
                histogram.AddObject(o.typeSizeStacktraceIndex, 1);
            }

            return histogram;
        }

        private void FillInNumbers()
        {
            allocatedBytesValueLabel.Text = CalculateTotalSize(logResult.allocatedHistogram);
            relocatedBytesValueLabel.Text = CalculateTotalSize(logResult.relocatedHistogram);
            finalHeapBytesValueLabel.Text = CalculateTotalSize(GetFinalHeapHistogram());

            gen0CollectionsValueLabel.Text = FormatNumber(logResult.liveObjectTable.lastGcGen0Count);
            gen1CollectionsValueLabel.Text = FormatNumber(logResult.liveObjectTable.lastGcGen1Count);
            gen2CollectionsValueLabel.Text = FormatNumber(logResult.liveObjectTable.lastGcGen2Count);
            inducedCollectionsValueLabel.Text = "Unknown";

            gen0HeapBytesValueLabel.Text = "Unknown";
            gen1HeapBytesValueLabel.Text = "Unknown";
            gen2HeapBytesValueLabel.Text = "Unknown";
            objectsFinalizedValueLabel.Text =  "Unknown";
            criticalObjectsFinalizedValueLabel.Text = "Unknown";
            largeObjectHeapBytesValueLabel.Text = "Unknown";
            if (log.gcCount[0] > 0)
            {
                objectsFinalizedValueLabel.Text =  CalculateTotalCount(logResult.finalizerHistogram);
                criticalObjectsFinalizedValueLabel.Text = CalculateTotalCount(logResult.criticalFinalizerHistogram);
                inducedCollectionsValueLabel.Text = FormatNumber(log.inducedGcCount[0]);
                gen0HeapBytesValueLabel.Text = FormatNumber(log.cumulativeGenerationSize[0] / (uint)log.gcCount[0]);
                if (log.gcCount[1] > 0)
                {
                    gen1HeapBytesValueLabel.Text = FormatNumber(log.cumulativeGenerationSize[1] / (uint)log.gcCount[1]);
                }
                else
                {
                    gen1HeapBytesValueLabel.Text = FormatNumber(log.generationSize[1]);
                }

                if (log.gcCount[2] > 0)
                {
                    gen2HeapBytesValueLabel.Text = FormatNumber(log.cumulativeGenerationSize[2] / (uint)log.gcCount[2]);
                }
                else
                {
                    gen2HeapBytesValueLabel.Text = FormatNumber(log.generationSize[2]);
                }

                if (log.gcCount[3] > 0)
                {
                    largeObjectHeapBytesValueLabel.Text = FormatNumber(log.cumulativeGenerationSize[3] / (uint)log.gcCount[3]);
                }
                else
                {
                    largeObjectHeapBytesValueLabel.Text = FormatNumber(log.generationSize[3]);
                }
            }
            else if (!logResult.createdHandlesHistogram.Empty)
            {
                // we know this is a new format log file
                // log.gcCount[0] was zero because there were no collections
                // in that case we know there were no induced collections and no finalized objects
                inducedCollectionsValueLabel.Text = "0";
                objectsFinalizedValueLabel.Text =  "0";
                criticalObjectsFinalizedValueLabel.Text = "0";
            }

            if (logResult.createdHandlesHistogram.Empty)
            {
                handlesCreatedValueLabel.Text = "Unknown";
                handlesDestroyedValueLabel.Text = "Unknown";
                handlesSurvivingValueLabel.Text = "Unknown";
            }
            else
            {
                handlesCreatedValueLabel.Text = CalculateTotalCount(logResult.createdHandlesHistogram);
                handlesDestroyedValueLabel.Text = CalculateTotalCount(logResult.destroyedHandlesHistogram);
                int count = 0;
                foreach (HandleInfo handleInfo in logResult.handleHash.Values)
                {
                    count++;
                }

                handlesSurvivingValueLabel.Text = FormatNumber(count);
            }

            commentsValueLabel.Text = FormatNumber(log.commentEventList.count);
            heapDumpsValueLabel.Text = FormatNumber(log.heapDumpEventList.count);
        }

        private void EnableDisableButtons()
        {
            allocatedHistogramButton.Enabled = !logResult.allocatedHistogram.Empty;
            allocationGraphButton.Enabled = !logResult.allocatedHistogram.Empty;
            relocatedHistogramButton.Enabled = !logResult.relocatedHistogram.Empty;
            finalizedHistogramButton.Enabled = !logResult.finalizerHistogram.Empty;
            criticalFinalizedHistogramButton.Enabled = !logResult.criticalFinalizerHistogram.Empty;
            handleAllocationGraphButton.Enabled = !logResult.createdHandlesHistogram.Empty;
            survingHandlesAllocationGraphButton.Enabled = logResult.handleHash.Count > 0;
            heapGraphButton.Enabled = log.heapDumpEventList.count > 0;
            commentsButton.Enabled = log.commentEventList.count > 0;
        }

        internal SummaryForm(ReadNewLog log, ReadLogResult logResult, string scenario)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.log = log;
            this.logResult = logResult;
            this.scenario = scenario;
            this.Text = "Summary for " + scenario;

            FillInNumbers();

            EnableDisableButtons();
        }


        private void allocatedHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Allocated Objects for: " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(logResult.allocatedHistogram, title);
            histogramViewForm.Show();
        }

        private void allocationGraphButton_Click(object sender, System.EventArgs e)
        {
            Graph graph = logResult.allocatedHistogram.BuildAllocationGraph(new FilterForm());
            graph.graphType = Graph.GraphType.AllocationGraph;
            string title = "Allocation Graph for: " + scenario;
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Show();
        }

        private void relocatedHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Relocated Objects for " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(logResult.relocatedHistogram, title);
            histogramViewForm.Show();        
        }

        private void finalHeapHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Surviving Objects for " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(GetFinalHeapHistogram(), title);
            histogramViewForm.Show();
        }

        private void finalHeapHistogramByAgeButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Age for Live Objects for " + scenario;
            AgeHistogram ageHistogram = new AgeHistogram(logResult.liveObjectTable, title);
            ageHistogram.Show();
        }

        private void finalHeapObjectsByAddressButton_Click(object sender, System.EventArgs e)
        {
            ViewByAddressForm viewByAddressForm = new ViewByAddressForm();
            viewByAddressForm.Visible = true;
        }

        private void finalizedHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Finalized Objects for " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(logResult.finalizerHistogram, title);
            histogramViewForm.Show();
        }

        private void criticalFinalizedHistogramButton_Click(object sender, System.EventArgs e)
        {
            string title = "Histogram by Size for Finalized Objects for " + scenario;
            HistogramViewForm histogramViewForm = new HistogramViewForm(logResult.criticalFinalizerHistogram, title);
            histogramViewForm.Show();        
        }

        private void timeLineButton_Click(object sender, System.EventArgs e)
        {
            TimeLineViewForm timeLineViewForm = new TimeLineViewForm();
            timeLineViewForm.Visible = true;
        }

        private void heapGraphButton_Click(object sender, System.EventArgs e)
        {
            Graph graph = logResult.objectGraph.BuildTypeGraph(new FilterForm());
            string title = "Heap Graph for " + scenario;
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Show();
        }

        private void commentsButton_Click(object sender, System.EventArgs e)
        {
            ViewCommentsForm viewCommentsForm = new ViewCommentsForm(log);
            viewCommentsForm.Visible = true;
        }

        private void CreateHandleAllocationGraph(Histogram histogram, string title)
        {
            Graph graph = histogram.BuildHandleAllocationGraph(new FilterForm());
            graph.graphType = Graph.GraphType.HandleAllocationGraph;
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Show();        
        }

        private void handleAllocationGraphButton_Click(object sender, System.EventArgs e)
        {
            string title = "Handle Allocation Graph for: " + scenario;
            CreateHandleAllocationGraph(logResult.createdHandlesHistogram, title);
        }

        private void survingHandlesAllocationGraphButton_Click(object sender, System.EventArgs e)
        {
            Histogram histogram = new Histogram(log);
            foreach (HandleInfo handleInfo in logResult.handleHash.Values)
            {   
                histogram.AddObject(handleInfo.allocStacktraceId, 1);
            }
            string title = "Surviving Handle Allocation Graph for: " + scenario;
            CreateHandleAllocationGraph(histogram, title);
        }
        
        private void copyMenuItem_Click(object sender, System.EventArgs e)
        {
            Label[] copyLabel = new Label []
            {
                allocatedBytesLabel,           allocatedBytesValueLabel,
                relocatedBytesLabel,           relocatedBytesValueLabel,
                finalHeapBytesLabel,           finalHeapBytesValueLabel,
                objectsFinalizedLabel,         objectsFinalizedValueLabel,
                criticalObjectsFinalizedLabel, criticalObjectsFinalizedValueLabel,
                gen0CollectionsLabel,          gen0CollectionsValueLabel,
                gen1CollectionsLabel,          gen1CollectionsValueLabel,
                gen2CollectionsLabel,          gen2CollectionsValueLabel,
                inducedCollectionsLabel,       inducedCollectionsValueLabel,
                gen0HeapBytesLabel,            gen0HeapBytesValueLabel,
                gen1HeapBytesLabel,            gen1HeapBytesValueLabel,
                gen2HeapBytesLabel,            gen2HeapBytesValueLabel,
                largeObjectHeapBytesLabel,     largeObjectHeapBytesValueLabel,
                handlesCreatedLabel,           handlesCreatedValueLabel,
                handlesDestroyedLabel,         handlesDestroyedValueLabel,
                handlesSurvivingLabel,         handlesSurvivingValueLabel,
                heapDumpsLabel,                heapDumpsValueLabel,
                commentsLabel,                 commentsValueLabel,
            };

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Summary for {0}\r\n", scenario);
            for (int i = 0; i < copyLabel.Length; i += 2)
            {
                sb.AppendFormat("{0,-30}{1,13}\r\n", copyLabel[i].Text, copyLabel[i+1].Text);
            }

            Clipboard.SetDataObject(sb.ToString());
        }
    }
}
