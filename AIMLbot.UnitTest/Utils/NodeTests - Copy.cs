using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Xml;
using AIMLbot.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AIMLbot.UnitTest.Utils
{
    [TestClass]
    public class NodeTests
    {
        private ChatBot _chatBot;

        private Node _node;

        private Request _request;

        private SubQuery _subQuery;

        [TestInitialize]
        public void Setup()
        {
            _chatBot = new ChatBot();
            ConfigurationManager.AppSettings["timeoutMax"] = Int32.MaxValue.ToString();
            _node = new Node();
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery("Test 1 <that> * <topic> *");
        }

        [TestMethod]
        public void TestAddCategoryAsLeafNode()
        {
            var path = "";
            var template = "<srai>TEST</srai>";
            _node.AddCategory(path, template);
            _node.Word = "*";

            Assert.AreEqual(0, _node.NumberOfChildNodes);
            Assert.AreEqual(template, _node.Template);
            Assert.AreEqual("*", _node.Word);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void TestAddCategoryWithEmptyTemplate()
        {
            var path = "Test 1 <that> * <topic> *";
            _node.AddCategory(path, string.Empty);
        }

        [TestMethod]
        public void TestAddCategoryWithGoodData()
        {
            var path = "Test 1 <that> * <topic> *";
            var template = "<srai>Test</srai>";
            _node = new Node();
            _node.AddCategory(path, template);

            Assert.AreEqual(1, _node.NumberOfChildNodes);
            Assert.AreEqual(string.Empty, _node.Template);
            Assert.AreEqual(string.Empty, _node.Word);
        }

        [TestMethod]
        public void TestEvaluateTimeOut()
        {
            _chatBot = new ChatBot();
            ConfigurationManager.AppSettings["timeoutMax"] = "10";
            _node = new Node();
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);

            var path = "Test 1 <that> that <topic> topic";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _subQuery = new SubQuery(path);

            Thread.Sleep(20);

            var result =
                _node.Evaluate("Test 1 <that> that <topic> topic", _subQuery, _request, MatchState.UserInput,
                    new StringBuilder());
            Assert.AreEqual(string.Empty, result);
            Assert.AreEqual(true, _request.HasTimedOut);
        }

        [TestMethod]
        public void testEvaluateWith_WildCardThat()
        {
            var path = "Test 1 <that> _ <topic> topic";
            var template = "<srai>TEST</srai>";
            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            var result = _node.Evaluate("Test 1 <that> WILDCARD WORDS <topic> topic", _subQuery, _request,
    MatchState.UserInput, new StringBuilder());

            Assert.AreEqual("<srai>TEST</srai>", result);
            Assert.AreEqual("WILDCARD WORDS", _subQuery.ThatStar[0]);
        }

        [TestMethod]
        public void testEvaluateWith_WildCardThatNotMatched()
        {
            var path = "Test 1 <that> _ <topic> topic";
            var template = "<srai>TEST</srai>";
            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST ALT</srai>",
                _node.Evaluate("Alt Test <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void testEvaluateWith_WildCardTopic()
        {
            var path = "Test 1 <that> that <topic> _";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);

            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);

            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate("Test 1 <that> that <topic> WILDCARD WORDS", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
            Assert.AreEqual("WILDCARD WORDS", _subQuery.TopicStar[0]);
        }

        [TestMethod]
        public void testEvaluateWith_WildCardTopicNotMatched()
        {
            var path = "Test 1 <that> that <topic> _";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST ALT</srai>",
                _node.Evaluate("Alt Test <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void TestEvaluateWith_WildCardUserInput()
        {
            var path = "_ Test 1 <that> that <topic> topic";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);

            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);

            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate("WILDCARD WORDS Test 1 <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
            Assert.AreEqual("WILDCARD WORDS", _subQuery.InputStar[0]);
        }

        [TestMethod]
        public void TestEvaluateWith_WildCardUserInputNotMatched()
        {
            var path = "_ Test 1 <that> that <topic> topic";
            var template = "<srai>TEST</srai>";

            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST ALT</srai>",
                _node.Evaluate("Alt Test <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void TestEvaluateWithEmptyNode()
        {
            _chatBot = new ChatBot();
            _node = new Node();
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery("Test 1 <that> that <topic> topic");

            Assert.AreEqual(string.Empty,
                _node.Evaluate("Test 1 <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void TestEvaluateWithInternationalCharset()
        {
            var path = "中 文 <that> * <topic> *";
            var template = "中文 (Chinese)";

            _node = new Node();
            _node.AddCategory(path, template);

            var path2 = "日 本 語 <that> * <topic> *";
            var template2 = "日 本 語 (Japanese)";

            _node.AddCategory(path2, template2);

            var path3 = "Русский язык <that> * <topic> *";
            var template3 = "Русский язык (Russian)";

            _node.AddCategory(path3, template3);

            _request = new Request("中 文", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("中文 (Chinese)",
                _node.Evaluate("中 文 <that> * <topic> *", _subQuery, _request, MatchState.UserInput,
                    new StringBuilder()));

            _request = new Request("日 本 語", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("日 本 語 (Japanese)",
                _node.Evaluate("日 本 語 <that> * <topic> *", _subQuery, _request, MatchState.UserInput,
                    new StringBuilder()));

            _request = new Request("Русский язык", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("Русский язык (Russian)",
                _node.Evaluate("Русский язык <that> * <topic> *", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void TestEvaluateWithMultipleWildcards()
        {
            var path = "Test _ 1 * <that> Test _ 1 * <topic> Test * 1 _";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);

            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);

            _subQuery = new SubQuery(path);

            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate(
                    "Test FIRST USER 1 SECOND USER <that> Test FIRST THAT 1 SECOND THAT <topic> Test FIRST TOPIC 1 SECOND TOPIC",
                    _subQuery, _request, MatchState.UserInput, new StringBuilder()));
            Assert.AreEqual(2, _subQuery.InputStar.Count);
            Assert.AreEqual("SECOND USER", _subQuery.InputStar[0]);
            Assert.AreEqual("FIRST USER", _subQuery.InputStar[1]);
            Assert.AreEqual(2, _subQuery.ThatStar.Count);
            Assert.AreEqual("SECOND THAT", _subQuery.ThatStar[0]);
            Assert.AreEqual("FIRST THAT", _subQuery.ThatStar[1]);
            Assert.AreEqual(2, _subQuery.TopicStar.Count);
            Assert.AreEqual("SECOND TOPIC", _subQuery.TopicStar[0]);
            Assert.AreEqual("FIRST TOPIC", _subQuery.TopicStar[1]);
        }

        [TestMethod]
        public void TestEvaluateWithMultipleWildcardsSwitched()
        {
            var path = "Test * 1 _ <that> Test * 1 _ <topic> Test _ 1 *";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);

            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);

            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate(
                    "Test FIRST USER 1 SECOND USER <that> Test FIRST THAT 1 SECOND THAT <topic> Test FIRST TOPIC 1 SECOND TOPIC",
                    _subQuery, _request, MatchState.UserInput, new StringBuilder()));
            Assert.AreEqual(2, _subQuery.InputStar.Count);
            Assert.AreEqual("SECOND USER", _subQuery.InputStar[0]);
            Assert.AreEqual("FIRST USER", _subQuery.InputStar[1]);
            Assert.AreEqual(2, _subQuery.ThatStar.Count);
            Assert.AreEqual("SECOND THAT", _subQuery.ThatStar[0]);
            Assert.AreEqual("FIRST THAT", _subQuery.ThatStar[1]);
            Assert.AreEqual(2, _subQuery.TopicStar.Count);
            Assert.AreEqual("SECOND TOPIC", _subQuery.TopicStar[0]);
            Assert.AreEqual("FIRST TOPIC", _subQuery.TopicStar[1]);
        }

        [TestMethod]
        public void TestEvaluateWithNoWildCards()
        {
            var path = "Test 1 <that> that <topic> topic";
            var template = "<srai>TEST</srai>";
            _node = new Node();
            _node.AddCategory(path, template);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate("Test 1 <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void TestEvaluateWithStarWildCardThat()
        {
            var path = "Test 1 <that> Test * 1 <topic> topic";
            var template = "<srai>TEST</srai>";
            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate("Test 1 <that> Test WILDCARD WORDS 1 <topic> topic", _subQuery,
                    _request, MatchState.UserInput, new StringBuilder()));
            Assert.AreEqual("WILDCARD WORDS", _subQuery.ThatStar[0]);
        }

        [TestMethod]
        public void TestEvaluateWithStarWildCardThatNotMatched()
        {
            var path = "Test 1 <that> Test * 1 <topic> topic";
            var template = "<srai>TEST</srai>";
            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST ALT</srai>",
                _node.Evaluate("Alt Test <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void TestEvaluateWithStarWildCardTopic()
        {
            var path = "Test 1 <that> that <topic> Test * 1";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);

            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);

            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate("Test 1 <that> that <topic> Test WILDCARD WORDS 1", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
            Assert.AreEqual("WILDCARD WORDS", _subQuery.TopicStar[0]);
        }

        [TestMethod]
        public void TestEvaluateWithStarWildCardTopicNotMatched()
        {
            var path = "Test 1 <that> that <topic> Test * 1";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST ALT</srai>",
                _node.Evaluate("Alt Test <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void TestEvaluateWithStarWildCardUserInput()
        {
            var path = "Test * 1 <that> that <topic> topic";
            var template = "<srai>TEST</srai>";
            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);

            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);

            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate("Test WILDCARD WORDS 1 <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
            Assert.AreEqual("WILDCARD WORDS", _subQuery.InputStar[0]);
        }

        [TestMethod]
        public void TestEvaluateWithStarWildCardUserInputNotMatched()
        {
            var path = "Test * 1 <that> that <topic> topic>";
            var template = "<srai>TEST</srai>";
            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);
            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);
            Assert.AreEqual("<srai>TEST ALT</srai>",
                _node.Evaluate("Alt Test <that> that <topic> topic", _subQuery, _request,
                    MatchState.UserInput, new StringBuilder()));
        }

        [TestMethod]
        public void TestEvaluateWithWildcardsInDifferentPartsOfPath()
        {
            var path = "Test * 1 <that> Test * 1 <topic> Test * 1";
            var template = "<srai>TEST</srai>";

            _node = new Node();
            _node.AddCategory(path, template);

            var pathAlt = "Alt Test <that> that <topic> topic";
            var templateAlt = "<srai>TEST ALT</srai>";

            _node.AddCategory(pathAlt, templateAlt);

            _request = new Request("Test 1", new User("1", _chatBot), _chatBot);
            _subQuery = new SubQuery(path);

            Assert.AreEqual("<srai>TEST</srai>",
                _node.Evaluate(
                    "Test WILDCARD USER WORDS 1 <that> Test WILDCARD THAT WORDS 1 <topic> Test WILDCARD TOPIC WORDS 1",
                    _subQuery, _request, MatchState.UserInput, new StringBuilder()));
            Assert.AreEqual("WILDCARD USER WORDS", _subQuery.InputStar[0]);
            Assert.AreEqual("WILDCARD THAT WORDS", _subQuery.ThatStar[0]);
            Assert.AreEqual("WILDCARD TOPIC WORDS", _subQuery.TopicStar[0]);
        }
    }
}