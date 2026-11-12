import { useState, useEffect } from 'react';
import { Wallet, TrendingUp, TrendingDown, RefreshCw, Send, Bot, User } from 'lucide-react';
import './App.css';

function App() {
  const [transacoes, setTransacoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState(null);

  // Estados do Chat
  const [mensagens, setMensagens] = useState([
    { autor: 'bot', texto: 'Olá! Sou seu assistente financeiro. Como posso ajudar você hoje?' }
  ]);
  const [pergunta, setPergunta] = useState('');
  const [enviando, setEnviando] = useState(false);

  // Função para buscar as transações da API .NET
  const carregarDados = async () => {
    setLoading(true);
    setErro(null);
    try {
      const response = await fetch('https://localhost:7184/api/Transacoes');
      if (!response.ok) throw new Error('Falha ao conectar com a API');
      const data = await response.json();
      setTransacoes(data);
    } catch (err) {
      setErro('Certifique-se de que a API .NET está rodando em https://localhost:7184');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregarDados();
  }, []);

  // Enviar Pergunta para a IA (/api/Chat/perguntar)
  const handleEnviarPergunta = async (e) => {
    e.preventDefault();
    if (!pergunta.trim() || enviando) return;

    const textoUsuario = pergunta;
    setPergunta('');
    setMensagens((prev) => [...prev, { autor: 'user', texto: textoUsuario }]);
    setEnviando(true);

    try {
      const response = await fetch('https://localhost:7184/api/Chat/perguntar', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mensagem: textoUsuario }),
      });

      if (!response.ok) throw new Error('Erro ao obter resposta da IA');
      
      const data = await response.json();
      setMensagens((prev) => [
        ...prev,
        { autor: 'bot', texto: data.resposta || data }
      ]);
    } catch (err) {
      setMensagens((prev) => [
        ...prev,
        { autor: 'bot', texto: 'Desculpe, ocorreu um erro ao se comunicar com a IA.' }
      ]);
      console.error(err);
    } finally {
      setEnviando(false);
    }
  };

 // Cálculos financeiros (1 = Receita, 2 = Despesa)
  const receitas = transacoes
    .filter((t) => t.tipo === 1 || t.tipo === 'Receita')
    .reduce((acc, t) => acc + t.valor, 0);

  const despesas = transacoes
    .filter((t) => t.tipo === 2 || t.tipo === 'Despesa')
    .reduce((acc, t) => acc + t.valor, 0);

  const saldoTotal = receitas - despesas;

  return (
    <div className="container">
      <header className="header">
        <h1>💰 Controle Financeiro IA</h1>
        <button className="btn-refresh" onClick={carregarDados}>
          <RefreshCw size={18} /> Atualizar
        </button>
      </header>

      {erro && <div className="card-erro">{erro}</div>}

      {/* Cartões de Resumo */}
      <div className="grid-cards">
        <div className="card saldo">
          <div className="card-header">
            <span>Saldo Consolidado</span>
            <Wallet size={24} />
          </div>
          <h2>R$ {saldoTotal.toFixed(2)}</h2>
        </div>

        <div className="card receita">
          <div className="card-header">
            <span>Total Receitas</span>
            <TrendingUp size={24} />
          </div>
          <h2>R$ {receitas.toFixed(2)}</h2>
        </div>

        <div className="card despesa">
          <div className="card-header">
            <span>Total Despesas</span>
            <TrendingDown size={24} />
          </div>
          <h2>R$ {despesas.toFixed(2)}</h2>
        </div>
      </div>

      {/* Tabela de Transações */}
      <section className="section-transacoes">
        <h2>Últimas Transações</h2>
        {loading ? (
          <p>Carregando dados da API...</p>
        ) : transacoes.length === 0 ? (
          <p>Nenhuma transação cadastrada.</p>
        ) : (
          <table className="tabela">
            <thead>
              <tr>
                <th>Data</th>
                <th>Descrição</th>
                <th>Tipo</th>
                <th>Valor</th>
              </tr>
            </thead>
            <tbody>
              {transacoes.map((t) => {
                const isReceita = t.tipo === 1 || t.tipo === 'Receita';
                const dataFormatada = new Date(t.data || t.dataTransacao).toLocaleDateString('pt-BR');

                return (
                  <tr key={t.id}>
                    <td>{dataFormatada}</td>
                    <td>{t.descricao}</td>
                    <td>
                      <span className={`tag ${isReceita ? 'tag-receita' : 'tag-despesa'}`}>
                        {isReceita ? 'Receita' : 'Despesa'}
                      </span>
                    </td>
                    <td>R$ {t.valor.toFixed(2)}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </section>

      {/* Seção do Chat com IA */}
      <section className="section-chat">
        <h2>🤖 Assistente Financeiro IA</h2>
        <div className="chat-box">
          <div className="historico-mensagens">
            {mensagens.map((m, idx) => (
              <div key={idx} className={`msg-wrapper ${m.autor}`}>
                <div className="avatar">
                  {m.autor === 'bot' ? <Bot size={20} /> : <User size={20} />}
                </div>
                <div className="balao-msg">{m.texto}</div>
              </div>
            ))}
            {enviando && (
              <div className="msg-wrapper bot">
                <div className="avatar"><Bot size={20} /></div>
                <div className="balao-msg digitando">Pensando...</div>
              </div>
            )}
          </div>

          <form onSubmit={handleEnviarPergunta} className="form-chat">
            <input
              type="text"
              placeholder="Pergunte algo ao assistente (ex: Quanto gastei com almoço?)..."
              value={pergunta}
              onChange={(e) => setPergunta(e.target.value)}
              disabled={enviando}
            />
            <button type="submit" disabled={enviando || !pergunta.trim()}>
              <Send size={18} />
            </button>
          </form>
        </div>
      </section>
    </div>
  );
}

export default App;