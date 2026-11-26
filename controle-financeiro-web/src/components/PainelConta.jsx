import { useState, useEffect, useCallback } from 'react';
import {
  ArrowLeft,
  Landmark,
  CreditCard,
  Plus,
  CheckCircle2,
  Clock,
  TrendingUp,
  TrendingDown,
  Pencil,
  Trash2,
} from 'lucide-react';
import { api } from '../services/api';
import { ModalEditarTransacao } from './ModalEditarTransacao';

const formatarMoeda = (valor) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor || 0);

const formatarDataBr = (isoDate) => {
  if (!isoDate) return '—';
  const [ano, mes, dia] = isoDate.split('T')[0].split('-');
  return `${dia}/${mes}/${ano}`;
};

const obterHojeISO = () => {
  const hoje = new Date();
  const ano = hoje.getFullYear();
  const mes = String(hoje.getMonth() + 1).padStart(2, '0');
  const dia = String(hoje.getDate()).padStart(2, '0');
  return `${ano}-${mes}-${dia}`;
};

export function PainelConta({ contaId, onVoltar, onAtualizarDashboard }) {
  const [extrato, setExtrato] = useState(null);
  const [categorias, setCategorias] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState(null);

  // Estado do formulário rápido de lançamento na conta
  const [mostrarForm, setMostrarForm] = useState(false);
  const [descricao, setDescricao] = useState('');
  const [valor, setValor] = useState('');
  const [tipo, setTipo] = useState(2); // 1 = Receita, 2 = Despesa
  const [categoriaId, setCategoriaId] = useState('');
  const [dataPrevista, setDataPrevista] = useState(obterHojeISO);
  const [compensado, setCompensado] = useState(false);
  const [dataCompensacao, setDataCompensacao] = useState(obterHojeISO);
  const [salvando, setSalvando] = useState(false);

  // Estado do Modal de Edição
  const [transacaoEditando, setTransacaoEditando] = useState(null);

  const carregarExtrato = useCallback(async () => {
    setCarregando(true);
    setErro(null);
    try {
      const [dadosExtrato, listaCategorias] = await Promise.all([
        api.obterExtratoConta(contaId),
        api.listarCategorias(),
      ]);
      setExtrato(dadosExtrato);
      setCategorias(listaCategorias);
      if (listaCategorias.length > 0 && !categoriaId) {
        setCategoriaId(String(listaCategorias[0].id));
      }
    } catch (err) {
      setErro(err.message || 'Erro ao carregar extrato da conta.');
    } finally {
      setCarregando(false);
    }
  }, [contaId, categoriaId]);

  useEffect(() => {
    carregarExtrato();
  }, [carregarExtrato]);

  const handleSalvarLancamento = async (e) => {
    e.preventDefault();
    setErro(null);

    const valorNumerico = parseFloat(String(valor).replace(/\./g, '').replace(',', '.'));
    if (Number.isNaN(valorNumerico) || valorNumerico <= 0) {
      setErro('Informe um valor válido maior que zero.');
      return;
    }

    const hojeISO = obterHojeISO();
    const dataCompFinal = compensado ? dataCompensacao || dataPrevista : null;

    if (compensado && dataCompFinal > hojeISO) {
      const confirmarFuturo = window.confirm(
        `A data de compensação (${formatarDataBr(dataCompFinal)}) é uma data futura.\n\nTem certeza de que este lançamento já está compensado no extrato?`
      );
      if (!confirmarFuturo) return;
    }

    setSalvando(true);
    try {
      await api.criarTransacao({
        descricao: descricao.trim(),
        valor: valorNumerico,
        tipo: Number(tipo),
        data: dataPrevista,
        dataCompensacao: dataCompFinal,
        isPago: compensado,
        contaId: Number(contaId),
        categoriaId: Number(categoriaId),
      });

      setDescricao('');
      setValor('');
      setCompensado(false);
      setMostrarForm(false);
      await carregarExtrato();
      if (onAtualizarDashboard) onAtualizarDashboard();
    } catch (err) {
      setErro(err.message || 'Erro ao registrar lançamento.');
    } finally {
      setSalvando(false);
    }
  };

  const handleExcluirLancamento = async (transacao) => {
    const confirmar = window.confirm(
      `Deseja realmente excluir o lançamento "${transacao.descricao}" (${formatarMoeda(transacao.valor)})?`
    );
    if (!confirmar) return;

    setErro(null);
    try {
      await api.excluirTransacao(transacao.id);
      await carregarExtrato();
      if (onAtualizarDashboard) onAtualizarDashboard();
    } catch (err) {
      setErro(err.message || 'Erro ao excluir lançamento.');
    }
  };

  if (carregando && !extrato) {
    return <p>Carregando extrato individual da conta...</p>;
  }

  if (!extrato) {
    return (
      <div>
        <button className="btn-secundario" onClick={onVoltar}>
          <ArrowLeft size={16} /> Voltar ao Dashboard
        </button>
        {erro && <div className="card-erro">{erro}</div>}
      </div>
    );
  }

  const { conta } = extrato;
  const isCartao = conta.tipoConta === 3;

  return (
    <section className="painel-conta">
      {/* Barra Superior do Painel Individual */}
      <div className="painel-conta-topo">
        <button type="button" className="btn-secundario btn-voltar" onClick={onVoltar}>
          <ArrowLeft size={18} /> Voltar ao Dashboard
        </button>

        <button
          type="button"
          className="btn-primario"
          onClick={() => setMostrarForm((prev) => !prev)}
        >
          <Plus size={18} /> {mostrarForm ? 'Fechar Formulário' : 'Novo Lançamento nesta Conta'}
        </button>
      </div>

      {erro && <div className="card-erro">{erro}</div>}

      {/* Cabeçalho com Identificação e Saldos da Conta */}
      <div className="card painel-conta-header">
        <div className="painel-conta-info">
          <div className="painel-conta-icone">
            {isCartao ? <CreditCard size={28} color="#fba94c" /> : <Landmark size={28} color="#00b37e" />}
          </div>
          <div>
            <h2>{conta.nome}</h2>
            <p className="subtexto-tabela">
              {conta.numBanco} - {conta.nomeBanco} • {conta.tipoContaDescricao} • Titular:{' '}
              {conta.titular}
              {!isCartao && (conta.agencia || conta.numConta)
                ? ` • Ag: ${conta.agencia || '-'} | Cc: ${conta.numConta || '-'}`
                : ''}
            </p>
          </div>
        </div>

        <div className="painel-conta-saldos">
          <div className="saldo-box">
            <span>Saldo Inicial</span>
            <strong>{formatarMoeda(extrato.saldoInicial)}</strong>
          </div>
          <div className="saldo-box">
            <span>{isCartao ? 'Fatura Atual' : 'Saldo Atual (Compensado)'}</span>
            <strong className={isCartao ? 'texto-alerta' : 'texto-positivo'}>
              {formatarMoeda(extrato.saldoAtual)}
            </strong>
          </div>
          <div className="saldo-box">
            <span>{isCartao ? 'Fatura Projetada' : 'Saldo Projetado'}</span>
            <strong className={isCartao ? 'texto-alerta' : 'texto-positivo'}>
              {formatarMoeda(extrato.saldoProjetado)}
            </strong>
          </div>
        </div>
      </div>

      {/* Formulário Rápido de Lançamento na Conta */}
      {mostrarForm && (
        <form onSubmit={handleSalvarLancamento} className="card form-lancamento-rapido">
          <h3>Novo Lançamento em {conta.nome}</h3>

          <div className="form-grid-lancamento">
            <div className="form-grupo">
              <label>Tipo</label>
              <div className="tipo-conta-selector">
                <button
                  type="button"
                  className={`tipo-btn ${Number(tipo) === 2 ? 'ativo' : ''}`}
                  onClick={() => setTipo(2)}
                >
                  <TrendingDown size={15} /> Despesa
                </button>
                <button
                  type="button"
                  className={`tipo-btn ${Number(tipo) === 1 ? 'ativo' : ''}`}
                  onClick={() => setTipo(1)}
                >
                  <TrendingUp size={15} /> Receita
                </button>
              </div>
            </div>

            <div className="form-grupo">
              <label>Descrição *</label>
              <input
                type="text"
                placeholder="Ex: Supermercado, Salário, Uber..."
                value={descricao}
                onChange={(e) => setDescricao(e.target.value)}
                required
              />
            </div>

            <div className="form-grupo">
              <label>Valor (R$) *</label>
              <input
                type="text"
                placeholder="0,00"
                value={valor}
                onChange={(e) => setValor(e.target.value)}
                required
              />
            </div>

            <div className="form-grupo">
              <label>Categoria</label>
              <select value={categoriaId} onChange={(e) => setCategoriaId(e.target.value)}>
                {categorias.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.nome}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-grupo">
              <label>Data Prevista</label>
              <input
                type="date"
                className="input-data-corte"
                value={dataPrevista}
                onChange={(e) => {
                  setDataPrevista(e.target.value);
                  if (compensado) setDataCompensacao(e.target.value);
                }}
                required
              />
            </div>

            <div className="form-grupo">
              <label>Situação no Extrato</label>
              <div className="check-compensado">
                <input
                  id="chk-compensado"
                  type="checkbox"
                  checked={compensado}
                  onChange={(e) => {
                    const marcado = e.target.checked;
                    setCompensado(marcado);
                    if (marcado) setDataCompensacao(dataPrevista);
                  }}
                />
                <label htmlFor="chk-compensado">Já compensado no extrato</label>
              </div>
            </div>

            {compensado && (
              <div className="form-grupo">
                <label>Data de Compensação</label>
                <input
                  type="date"
                  className="input-data-corte"
                  value={dataCompensacao}
                  onChange={(e) => setDataCompensacao(e.target.value)}
                  required
                />
              </div>
            )}
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secundario" onClick={() => setMostrarForm(false)}>
              Cancelar
            </button>
            <button type="submit" className="btn-primario" disabled={salvando}>
              {salvando ? 'Salvando...' : 'Confirmar Lançamento'}
            </button>
          </div>
        </form>
      )}

      {/* Extrato Compensado (com Saldo Acumulado Linha a Linha) */}
      <div className="bloco-contas">
        <h3>
          <CheckCircle2 size={19} color="#00b37e" /> Lançamentos Compensados ({extrato.compensados.length})
        </h3>

        {extrato.compensados.length === 0 ? (
          <p className="texto-vazio">Nenhum lançamento compensado nesta conta.</p>
        ) : (
          <table className="tabela">
            <thead>
              <tr>
                <th>Compensação</th>
                <th>Ordem</th>
                <th>Descrição</th>
                <th>Categoria</th>
                <th>Valor</th>
                <th>{isCartao ? 'Fatura Acumulada' : 'Saldo Após'}</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {extrato.compensados.map((t) => {
                const isReceita = t.tipo === 1;
                return (
                  <tr key={t.id}>
                    <td>{formatarDataBr(t.dataCompensacao)}</td>
                    <td>#{t.ordem}</td>
                    <td>{t.descricao}</td>
                    <td>
                      <span className="conta-pill-tipo">{t.nomeCategoria}</span>
                    </td>
                    <td className={isReceita ? 'texto-positivo' : 'texto-negativo'}>
                      {isReceita ? '+' : '-'} {formatarMoeda(t.valor)}
                    </td>
                    <td>
                      <strong>{formatarMoeda(t.saldoAposLancamento)}</strong>
                    </td>
                    <td>
                      <div className="acoes-linha">
                        <button
                          type="button"
                          className="btn-icone"
                          title="Editar lançamento"
                          onClick={() => setTransacaoEditando(t)}
                        >
                          <Pencil size={15} />
                        </button>
                        <button
                          type="button"
                          className="btn-icone btn-icone-excluir"
                          title="Excluir lançamento"
                          onClick={() => handleExcluirLancamento(t)}
                        >
                          <Trash2 size={15} />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Lançamentos Futuros / Pendentes */}
      <div className="bloco-contas">
        <h3>
          <Clock size={19} color="#fba94c" /> Lançamentos Previstos / Pendentes ({extrato.futuros.length})
        </h3>

        {extrato.futuros.length === 0 ? (
          <p className="texto-vazio">Nenhum lançamento futuro pendente nesta conta.</p>
        ) : (
          <table className="tabela">
            <thead>
              <tr>
                <th>Data Prevista</th>
                <th>Descrição</th>
                <th>Categoria</th>
                <th>Valor</th>
                <th>Saldo Projetado</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {extrato.futuros.map((t) => {
                const isReceita = t.tipo === 1;
                return (
                  <tr key={t.id}>
                    <td>{formatarDataBr(t.data)}</td>
                    <td>{t.descricao}</td>
                    <td>
                      <span className="conta-pill-tipo">{t.nomeCategoria}</span>
                    </td>
                    <td className={isReceita ? 'texto-positivo' : 'texto-negativo'}>
                      {isReceita ? '+' : '-'} {formatarMoeda(t.valor)}
                    </td>
                    <td>
                      <strong>{formatarMoeda(t.saldoAposLancamento)}</strong>
                    </td>
                    <td>
                      <div className="acoes-linha">
                        <button
                          type="button"
                          className="btn-icone"
                          title="Editar lançamento"
                          onClick={() => setTransacaoEditando(t)}
                        >
                          <Pencil size={15} />
                        </button>
                        <button
                          type="button"
                          className="btn-icone btn-icone-excluir"
                          title="Excluir lançamento"
                          onClick={() => handleExcluirLancamento(t)}
                        >
                          <Trash2 size={15} />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Modal de Edição de Lançamento */}
      <ModalEditarTransacao
        transacao={transacaoEditando}
        categorias={categorias}
        onFechar={() => setTransacaoEditando(null)}
        onSalvo={async () => {
          await carregarExtrato();
          if (onAtualizarDashboard) onAtualizarDashboard();
        }}
        onExcluir={handleExcluirLancamento}
      />
    </section>
  );
}