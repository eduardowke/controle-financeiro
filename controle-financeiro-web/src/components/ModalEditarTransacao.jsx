import { useState, useEffect } from 'react';
import { X, TrendingUp, TrendingDown, Trash2 } from 'lucide-react';
import { api } from '../services/api';

const obterHojeISO = () => {
  const hoje = new Date();
  const ano = hoje.getFullYear();
  const mes = String(hoje.getMonth() + 1).padStart(2, '0');
  const dia = String(hoje.getDate()).padStart(2, '0');
  return `${ano}-${mes}-${dia}`;
};

const formatarDataBr = (isoDate) => {
  if (!isoDate) return '—';
  const [ano, mes, dia] = isoDate.split('T')[0].split('-');
  return `${dia}/${mes}/${ano}`;
};

export function ModalEditarTransacao({
  transacao,
  categorias,
  onFechar,
  onSalvo,
  onExcluir,
}) {
  const [descricao, setDescricao] = useState('');
  const [valor, setValor] = useState('');
  const [tipo, setTipo] = useState(2);
  const [categoriaId, setCategoriaId] = useState('');
  const [dataPrevista, setDataPrevista] = useState('');
  const [compensado, setCompensado] = useState(false);
  const [dataCompensacao, setDataCompensacao] = useState('');
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    if (!transacao) return;
    setDescricao(transacao.descricao || '');
    setValor(String(transacao.valor ?? '').replace('.', ','));
    setTipo(Number(transacao.tipo));
    setCategoriaId(String(transacao.categoriaId));
    const prevISO = transacao.data ? transacao.data.split('T')[0] : obterHojeISO();
    setDataPrevista(prevISO);

    const isComp = Boolean(transacao.dataCompensacao);
    setCompensado(isComp);
    setDataCompensacao(
      isComp ? transacao.dataCompensacao.split('T')[0] : prevISO
    );
    setErro(null);
  }, [transacao]);

  if (!transacao) return null;

  const handleSubmit = async (e) => {
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
      const confirmar = window.confirm(
        `A data de compensação (${formatarDataBr(dataCompFinal)}) é uma data futura.\n\nTem certeza de que este lançamento já está compensado no extrato?`
      );
      if (!confirmar) return;
    }

    setSalvando(true);
    try {
      await api.atualizarTransacao(transacao.id, {
        descricao: descricao.trim(),
        valor: valorNumerico,
        tipo: Number(tipo),
        data: dataPrevista,
        dataCompensacao: dataCompFinal,
        isPago: compensado,
        categoriaId: Number(categoriaId),
      });
      onSalvo();
      onFechar();
    } catch (err) {
      setErro(err.message || 'Erro ao salvar alterações.');
    } finally {
      setSalvando(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onFechar}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Editar Lançamento</h2>
          <button type="button" className="btn-fechar-modal" onClick={onFechar}>
            <X size={20} />
          </button>
        </div>

        {erro && <div className="card-erro">{erro}</div>}

        <form onSubmit={handleSubmit}>
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
              value={descricao}
              onChange={(e) => setDescricao(e.target.value)}
              required
            />
          </div>

          <div className="form-grid-2col">
            <div className="form-grupo">
              <label>Valor (R$) *</label>
              <input
                type="text"
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
          </div>

          <div className="form-grid-2col">
            <div className="form-grupo">
              <label>Data Prevista</label>
              <input
                type="date"
                className="input-data-corte"
                value={dataPrevista}
                onChange={(e) => setDataPrevista(e.target.value)}
                required
              />
            </div>

            <div className="form-grupo">
              <label>Situação no Extrato</label>
              <div className="check-compensado">
                <input
                  id="chk-edit-compensado"
                  type="checkbox"
                  checked={compensado}
                  onChange={(e) => {
                    const marcado = e.target.checked;
                    setCompensado(marcado);
                    if (marcado && !dataCompensacao) setDataCompensacao(dataPrevista);
                  }}
                />
                <label htmlFor="chk-edit-compensado">Já compensado no extrato</label>
              </div>
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

          <div className="modal-footer modal-footer-split">
            <button
              type="button"
              className="btn-acao-tabela btn-excluir"
              onClick={() => {
                onFechar();
                onExcluir(transacao);
              }}
            >
              <Trash2 size={15} /> Excluir
            </button>

            <div className="acoes-linha">
              <button type="button" className="btn-secundario" onClick={onFechar}>
                Cancelar
              </button>
              <button type="submit" className="btn-primario" disabled={salvando}>
                {salvando ? 'Salvando...' : 'Salvar Alterações'}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
}