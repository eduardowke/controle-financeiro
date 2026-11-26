import { useState, useEffect, useCallback } from 'react';
import {
  RefreshCw,
  Send,
  Bot,
  User,
  BrainCircuit,
  ShieldAlert,
  Lightbulb,
  PieChart,
  Plus,
} from 'lucide-react';
import { api } from './services/api';
import { ModalCadastroConta } from './components/ModalCadastroConta';
import { DashboardContas } from './components/DashboardContas';
import { PainelConta } from './components/PainelConta';
import './App.css';

function obterFimDoMesAtualISO() {
  const hoje = new Date();
  const fimMes = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0);
  return fimMes.toISOString().split('T')[0];
}

function App() {
  const [dataCorte, setDataCorte] = useState(obterFimDoMesAtualISO);
  const [resumoDashboard, setResumoDashboard] = useState(null);
  const [transacoes, setTransacoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState(null);
  const [modalContaAberto, setModalContaAberto] = useState(false);
  const [contaSelecionadaId, setContaSelecionadaId] = useState(null);

  // Estados do Chat
  const [mensagens, setMensagens] = useState([
    { autor: 'bot', texto: 'Olá! Sou seu assistente financeiro. Como posso ajudar você hoje?' },
  ]);
  const [pergunta, setPergunta] = useState('');
  const [enviando, setEnviando] = useState(false);

  // Estados da Análise Multiagente
  const [analiseMultiagente, setAnaliseMultiagente] = useState(null);
  const [gerandoAnalise, setGerandoAnalise] = useState(false);

  // Carrega resumo do dashboard (com projeção) e transações
  const carregarDados = useCallback(async (dataSelecionada = dataCorte) => {
    setLoading(true);
    setErro(null);
    try {
      const [resumo, listaTransacoes] = await Promise.all([
        api.obterResumoDashboard(dataSelecionada),
        api.listarTransacoes(),
      ]);
      setResumoDashboard(resumo);
      setTransacoes(listaTransacoes);
    } catch (err) {
      setErro(`Certifique-se de que a API .NET está rodando em ${api.baseUrl}`);
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [dataCorte]);

  useEffect(() => {
    carregarDados(dataCorte);
  }, [dataCorte, carregarDados]);

  const handleContaSalva = () => {
    carregarDados(dataCorte);
  };

  // Enviar Pergunta para a IA (/api/Chat/perguntar)
  const handleEnviarPergunta = async (e) => {
    e.preventDefault();
    if (!pergunta.trim() || enviando) return;

    const textoUsuario = pergunta;
    setPergunta('');
    setMensagens((prev) => [...prev, { autor: 'user', texto: textoUsuario }]);
    setEnviando(true);

    try {
      const response = await fetch(`${api.baseUrl}/Chat/perguntar`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mensagem: textoUsuario }),
      });

      if (!response.ok) throw new Error('Erro ao obter resposta da IA');

      const data = await response.json();
      setMensagens((prev) => [...prev, { autor: 'bot', texto: data.resposta || data }]);
    } catch (err) {
      setMensagens((prev) => [
        ...prev,
        { autor: 'bot', texto: 'Desculpe, ocorreu um erro ao se comunicar com a IA.' },
      ]);
      console.error(err);
    } finally {
      setEnviando(false);
    }
  };

  // Acionar Pipeline Multiagente (/api/Chat/analise-multiagente)
  const handleGerarAnaliseMultiagente = async () => {
    setGerandoAnalise(true);
    setAnaliseMultiagente(null);
    try {
      const response = await fetch(`${api.baseUrl}/Chat/analise-multiagente`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({}),
      });

      if (!response.ok) {
        const erroDetalhe = await response.text();
        throw new Error(`Erro ${response.status}: ${erroDetalhe}`);
      }

      const data = await response.json();
      setAnaliseMultiagente(data);
    } catch (err) {
      alert(`Falha na análise: ${err.message}`);
      console.error(err);
    } finally {
      setGerandoAnalise(false);
    }
  };

  return (
    <div className="container">
      <header className="header">
        <h1>💰 Controle Financeiro IA</h1>
        <div className="header-acoes">
          <button className="btn-primario" onClick={() => setModalContaAberto(true)}>
            <Plus size={18} /> Nova Conta
          </button>
          <button className="btn-refresh" onClick={() => carregarDados(dataCorte)}>
            <RefreshCw size={18} /> Atualizar
          </button>
        </div>
      </header>

      {erro && <div className="card-erro">{erro}</div>}

      {/* Novo Dashboard ou Painel Detalhado da Conta Selecionada */}
      {contaSelecionadaId !== null ? (
        <PainelConta
          contaId={contaSelecionadaId}
          onVoltar={() => setContaSelecionadaId(null)}
          onAtualizarDashboard={() => carregarDados(dataCorte)}
        />
      ) : (
        <DashboardContas
          resumo={resumoDashboard}
          dataCorte={dataCorte}
          onChangeDataCorte={setDataCorte}
          carregando={loading}
          onSelecionarConta={setContaSelecionadaId}
        />
      )}

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
                <div className="avatar">
                  <Bot size={20} />
                </div>
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

      {/* Seção de Consultoria Multiagente */}
      <section className="section-multiagente">
        <div className="multiagente-header">
          <div>
            <h2>🧠 Consultoria Financeira Multiagente</h2>
            <p className="subtitulo">
              Orquestração autônoma com 3 especialistas de IA analisando sua carteira.
            </p>
          </div>
          <button
            className="btn-multiagente"
            onClick={handleGerarAnaliseMultiagente}
            disabled={gerandoAnalise}
          >
            <BrainCircuit size={20} />
            {gerandoAnalise ? 'Executando Agentes...' : 'Gerar Diagnóstico Completo'}
          </button>
        </div>

        {analiseMultiagente && (
          <div className="grid-agentes">
            <div className="card-agente perfil">
              <div className="agente-titulo">
                <PieChart size={22} />
                <h3>1. Analista de Perfil</h3>
              </div>
              <p>{analiseMultiagente.perfilFinanceiroIdentificado}</p>
            </div>

            <div className="card-agente risco">
              <div className="agente-titulo">
                <ShieldAlert size={22} />
                <h3>2. Analista de Risco</h3>
              </div>
              <p>{analiseMultiagente.avaliacaoRiscoEConsumo}</p>
            </div>

            <div className="card-agente recomendacao">
              <div className="agente-titulo">
                <Lightbulb size={22} />
                <h3>3. Consultor de Estratégia</h3>
              </div>
              <p>{analiseMultiagente.recomendacoesEstrategicas}</p>
            </div>
          </div>
        )}
      </section>

      {/* Modal de Cadastro de Conta (US 266) */}
      <ModalCadastroConta
        aberto={modalContaAberto}
        onFechar={() => setModalContaAberto(false)}
        onContaSalva={handleContaSalva}
      />
    </div>
  );
}

export default App;