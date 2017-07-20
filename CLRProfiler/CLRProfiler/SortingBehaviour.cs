namespace CLRProfiler
{
    internal sealed class SortingBehaviour
    {
        internal int SortingOrder { get; set; }
        internal int CounterId { get; set; }

        public SortingBehaviour(int sortingOrder, int counterId)
        {
            SortingOrder = sortingOrder;
            CounterId = counterId;
        }
    }
}