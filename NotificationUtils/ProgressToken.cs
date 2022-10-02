namespace NotificationUtils
{
    public struct ProgressToken
    {
        public static readonly ProgressToken None;

        private ProgressNode node;

        internal ProgressToken(ProgressNode node)
        {
            this.node = node;
        }

        public ProgressToken CreateBranchedToken(int weight)
        {
            if (node is null)
            {
                return None;
            }
            else
            {
                return new ProgressToken(node?.CreateBranchedTree(weight));
            }
        }

        public ProgressLeaf CreateLeaf(int weight = 1, int max = 1, int notificationStep = 1)
        {
            if (node is null)
            {
                return new ProgressLeaf(weight, 0, max, notificationStep);
            }
            else
            {
                return node.CreateLeaf(weight, max, notificationStep);
            }
        }
    }
}
