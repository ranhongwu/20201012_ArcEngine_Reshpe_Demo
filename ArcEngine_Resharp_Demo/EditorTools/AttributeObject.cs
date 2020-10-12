namespace PS.Plot.Editor
{
    /// <summary>
    /// 暂存属性编辑的类
    /// </summary>
    public class AttributeEditObject
    {
        /// <summary>
        /// 编辑的要素ID
        /// </summary>
        public object FID
        {
            get; set;
        }

        /// <summary>
        /// 编辑的列号
        /// </summary>
        public string FieldName
        {
            get; set;
        }

        /// <summary>
        /// 编辑的内容
        /// </summary>
        public object Value
        {
            get; set;
        }
    }
}