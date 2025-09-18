using Scada.Server.Engine;

namespace ScriptPlayground
{
    /// <summary>
    /// A base class for testing scripts.
    /// <para>Базовый класс для тестирования скриптов.</para>
    /// </summary>
    public class ScriptTester : CalcEngine
    {
        public ScriptTester() 
            : base()
        {
            calcContext = new PlaygroundCalcContext();
        }
    }
}
