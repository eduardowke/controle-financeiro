import { useState, useEffect } from 'react';
import { X, Landmark, CreditCard, Wallet, TrendingUp, Sparkles } from 'lucide-react';
import { api } from '../services/api';

export function ModalCadastroConta({ aberto, onFechar, onContaSalva }) {
  const [bancos, setBancos] = useState([]);
  const [carregandoBancos, setCarregandoBancos] = useState(false);

  const [numBanco, setNumBanco] = useState('341'); // Itaú como padrão inicial
  const [tipoConta, setTipoConta] = useState(1); // 1 = Corrente, 2 = Investimento, 3 = Cartão
  const [titular, setTitular] = useState('');
  const [nome, setNome] = useState('');
  const [nomeEditadoManualmente, setNomeEditadoManualmente] = useState(false);
  const [agencia, setAgencia] = useState('');
  const [numConta, setNumConta] = useState('');
  const [saldoInicial, setSaldoInicial] = useState('0,00');

  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState(null);

  // Carrega lista de bancos brasileiros ao abrir o modal
  useEffect(() => {
    if (!aberto) return;

    const carregarBancos = async () => {
      setCarregandoBancos(true);
      try {
        const lista = await api.listarBancos();
        setBancos(lista);
      } catch (err) {
        setErro('Não foi possível carregar a lista de bancos.');
        console.error(err);
      } finally {
        setCarregandoBancos(false);
      }
    };

    carregarBancos();
  }, [aberto]);

  // Sugestão inteligente de nome: "{NomeReduzido} - {PrimeiroNome}"
  useEffect(() => {
    if (nomeEditadoManualmente) return;

    const bancoSelecionado = bancos.find((b) => b.numBanco === numBanco);
    const primeiroNome = titular.trim().split(/\s+/)[0] || '';

    if (bancoSelecionado && primeiroNome.length >= 2) {
      const primeiroNomeFormatado =
        primeiroNome.charAt(0).toUpperCase() + primeiroNome.slice(1).toLowerCase();
      setNome(`${bancoSelecionado.nomeReduzido} - ${primeiroNomeFormatado}`);
    } else if (bancoSelecionado && !primeiroNome) {
      setNome('');
    }
  }, [numBanco, titular, bancos, nomeEditadoManualmente]);

  if (!aberto) return null;

  const isCartaoCredito = Number(tipoConta) === 3;

  const limparFormulario = () => {
    setNumBanco('341');
    setTipoConta(1);
    setTitular('');
    setNome('');
    setNomeEditadoManualmente(false);
    setAgencia('');
    setNumConta('');
    setSaldoInicial('0,00');
    setErro(null);
  };

  const handleFechar = () => {
    limparFormulario();
    onFechar();
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErro(null);

    if (titular.trim().length < 3) {
      setErro('O nome do titular deve ter no mínimo 3 caracteres.');
      return;
    }

    // Converte formato brasileiro "1.250,50" ou "61,73" para float 61.73
    const valorNumerico = parseFloat(
      String(saldoInicial).replace(/\./g, '').replace(',', '.')
    );

    if (Number.isNaN(valorNumerico)) {
      setErro('Informe um saldo inicial válido.');
      return;
    }

    setSalvando(true);
    try {
      const payload = {
        nome: nome.trim(),
        numBanco,
        tipoConta: Number(tipoConta),
        titular: titular.trim(),
        agencia: isCartaoCredito ? null : agencia.trim() || null,
        numConta: isCartaoCredito ? null : numConta.trim() || null,
        saldoInicial: valorNumerico,
      };

      const novaConta = await api.criarConta(payload);
      onContaSalva(novaConta);
      handleFechar();
    } catch (err) {
      setErro(err.message || 'Erro ao cadastrar conta.');
    } finally {
      setSalvando(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={handleFechar}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div className="modal-titulo">
            <Landmark size={22} color="#00b37e" />
            <h2>Nova Conta / Cartão</h2>
          </div>
          <button type="button" className="btn-fechar-modal" onClick={handleFechar}>
            <X size={20} />
          </button>
        </div>

        {erro && <div className="card-erro">{erro}</div>}

        <form onSubmit={handleSubmit} className="form-modal">
          {/* Tipo de Conta */}
          <div className="form-grupo">
            <label>Tipo de Conta</label>
            <div className="tipo-conta-selector">
              <button
                type="button"
                className={`tipo-btn ${Number(tipoConta) === 1 ? 'ativo' : ''}`}
                onClick={() => setTipoConta(1)}
              >
                <Wallet size={16} /> Conta Corrente
              </button>
              <button
                type="button"
                className={`tipo-btn ${Number(tipoConta) === 2 ? 'ativo' : ''}`}
                onClick={() => setTipoConta(2)}
              >
                <TrendingUp size={16} /> Investimento
              </button>
              <button
                type="button"
                className={`tipo-btn ${Number(tipoConta) === 3 ? 'ativo' : ''}`}
                onClick={() => setTipoConta(3)}
              >
                <CreditCard size={16} /> Cartão de Crédito
              </button>
            </div>
          </div>

          {/* Instituição Bancária */}
          <div className="form-grupo">
            <label htmlFor="select-banco">Instituição Bancária</label>
            <select
              id="select-banco"
              value={numBanco}
              onChange={(e) => setNumBanco(e.target.value)}
              disabled={carregandoBancos}
              required
            >
              {bancos.map((b) => (
                <option key={b.numBanco} value={b.numBanco}>
                  {b.numBanco} - {b.nomeReduzido}
                </option>
              ))}
            </select>
          </div>

          {/* Titular */}
          <div className="form-grupo">
            <label htmlFor="input-titular">Nome do Titular *</label>
            <input
              id="input-titular"
              type="text"
              placeholder="Ex: Eduardo Gomes Tadeu Sobrinho"
              value={titular}
              onChange={(e) => setTitular(e.target.value)}
              minLength={3}
              required
            />
          </div>

          {/* Nome da Conta (com sugestão inteligente) */}
          <div className="form-grupo">
            <div className="label-com-dica">
              <label htmlFor="input-nome-conta">Nome de Exibição da Conta *</label>
              {!nomeEditadoManualmente && nome && (
                <span className="badge-sugestao">
                  <Sparkles size={12} /> Sugerido automaticamente
                </span>
              )}
            </div>
            <input
              id="input-nome-conta"
              type="text"
              placeholder="Ex: Itaú - Eduardo"
              value={nome}
              onChange={(e) => {
                setNome(e.target.value);
                setNomeEditadoManualmente(true);
              }}
              required
            />
          </div>

          {/* Campos Condicionais (Ocultos para Cartão de Crédito) */}
          {!isCartaoCredito && (
            <div className="form-linha-dupla">
              <div className="form-grupo">
                <label htmlFor="input-agencia">Agência (Opcional)</label>
                <input
                  id="input-agencia"
                  type="text"
                  placeholder="Ex: 7892"
                  maxLength={10}
                  value={agencia}
                  onChange={(e) => setAgencia(e.target.value)}
                />
              </div>

              <div className="form-grupo">
                <label htmlFor="input-num-conta">Número da Conta (Opcional)</label>
                <input
                  id="input-num-conta"
                  type="text"
                  placeholder="Ex: 23250-0"
                  maxLength={20}
                  value={numConta}
                  onChange={(e) => setNumConta(e.target.value)}
                />
              </div>
            </div>
          )}

          {/* Saldo Inicial */}
          <div className="form-grupo">
            <label htmlFor="input-saldo">
              {isCartaoCredito ? 'Fatura Atual em Aberto (R$)' : 'Saldo Inicial (R$)'}
            </label>
            <input
              id="input-saldo"
              type="text"
              placeholder="0,00"
              value={saldoInicial}
              onChange={(e) => setSaldoInicial(e.target.value)}
            />
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secundario" onClick={handleFechar}>
              Cancelar
            </button>
            <button type="submit" className="btn-primario" disabled={salvando}>
              {salvando ? 'Salvando...' : 'Salvar Conta'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}