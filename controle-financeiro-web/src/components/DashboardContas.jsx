import {
  Wallet,
  CalendarClock,
  CreditCard,
  TrendingUp,
  TrendingDown,
  Landmark,
  Clock,
} from 'lucide-react';

const formatarMoeda = (valor) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor || 0);

const formatarDataBr = (isoDate) => {
  if (!isoDate) return '';
  const [ano, mes, dia] = isoDate.split('T')[0].split('-');
  return `${dia}/${mes}/${ano}`;
};

export function DashboardContas({ resumo, dataCorte, onChangeDataCorte, carregando }) {
  const obterDataISO = (dateObj) => dateObj.toISOString().split('T')[0];

  const aplicarAtalhoData = (tipo) => {
    const hoje = new Date();
    if (tipo === 'hoje') {
      onChangeDataCorte(obterDataISO(hoje));
    } else if (tipo === 'fimMes') {
      const fimMes = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0);
      onChangeDataCorte(obterDataISO(fimMes));
    } else if (tipo === 'mais30') {
      const mais30 = new Date(hoje);
      mais30.setDate(hoje.getDate() + 30);
      onChangeDataCorte(obterDataISO(mais30));
    } else if (tipo === 'proximoMes') {
      const fimProxMes = new Date(hoje.getFullYear(), hoje.getMonth() + 2, 0);
      onChangeDataCorte(obterDataISO(fimProxMes));
    }
  };

  if (!resumo && carregando) {
    return <p>Carregando painel financeiro...</p>;
  }

  if (!resumo) return null;

  const variacaoLiquidez = resumo.liquidezProjetada - resumo.liquidezAtual;

  return (
    <section className="dashboard-contas">
      {/* Barra de Controle de Data de Projeção */}
      <div className="projecao-bar">
        <div className="projecao-titulo">
          <CalendarClock size={20} color="#00b37e" />
          <div>
            <strong>Projeção Financeira até:</strong>
            <span className="projecao-sub">
              Considera saldos compensados + lançamentos previstos até {formatarDataBr(dataCorte)}
            </span>
          </div>
        </div>

        <div className="projecao-controles">
          <div className="atalhos-data">
            <button type="button" onClick={() => aplicarAtalhoData('hoje')}>
              Hoje
            </button>
            <button type="button" onClick={() => aplicarAtalhoData('fimMes')}>
              Fim do Mês
            </button>
            <button type="button" onClick={() => aplicarAtalhoData('mais30')}>
              +30 dias
            </button>
            <button type="button" onClick={() => aplicarAtalhoData('proximoMes')}>
              Fim Próx. Mês
            </button>
          </div>

          <input
            type="date"
            className="input-data-corte"
            value={dataCorte}
            onChange={(e) => e.target.value && onChangeDataCorte(e.target.value)}
          />
        </div>
      </div>

      {/* Cards Consolidados: Liquidez vs Cartões */}
      <div className="grid-cards-dashboard">
        <div className="card saldo">
          <div className="card-header">
            <span>Liquidez Atual (Compensado)</span>
            <Wallet size={22} />
          </div>
          <h2>{formatarMoeda(resumo.liquidezAtual)}</h2>
          <span className="card-rodape">Contas Correntes e Investimentos</span>
        </div>

        <div className="card projetado">
          <div className="card-header">
            <span>Liquidez Projetada ({formatarDataBr(dataCorte)})</span>
            <CalendarClock size={22} />
          </div>
          <h2 className={resumo.liquidezProjetada < 0 ? 'texto-negativo' : 'texto-positivo'}>
            {formatarMoeda(resumo.liquidezProjetada)}
          </h2>
          <span className="card-rodape">
            Variação prevista: {variacaoLiquidez >= 0 ? '+' : ''}
            {formatarMoeda(variacaoLiquidez)}
          </span>
        </div>

        <div className="card cartao">
          <div className="card-header">
            <span>Faturas de Cartões (Atual)</span>
            <CreditCard size={22} />
          </div>
          <h2>{formatarMoeda(resumo.faturaCartoesAtual)}</h2>
          <span className="card-rodape">
            Projetado até {formatarDataBr(dataCorte)}:{' '}
            <strong>{formatarMoeda(resumo.faturaCartoesProjetada)}</strong>
          </span>
        </div>

        <div className="card resultado">
          <div className="card-header">
            <span>Fluxo Efetivo no Mês</span>
            <TrendingUp size={22} />
          </div>
          <div className="fluxo-linhas">
            <div className="fluxo-item receita">
              <TrendingUp size={15} />
              <span>Receitas: {formatarMoeda(resumo.receitasRealizadasPeriodo)}</span>
            </div>
            <div className="fluxo-item despesa">
              <TrendingDown size={15} />
              <span>Despesas: {formatarMoeda(resumo.despesasRealizadasPeriodo)}</span>
            </div>
          </div>
          <span className="card-rodape">
            Pendentes até corte: +{formatarMoeda(resumo.receitasPrevistasPeriodo)} / -
            {formatarMoeda(resumo.despesasPrevistasPeriodo)}
          </span>
        </div>
      </div>

      {/* Bloco 1: Contas Bancárias (Liquidez) */}
      <div className="bloco-contas">
        <h3>
          <Landmark size={19} color="#00b37e" /> Contas Bancárias (Liquidez)
        </h3>

        {resumo.contasBancarias.length === 0 ? (
          <p className="texto-vazio">Nenhuma conta corrente ou de investimento cadastrada.</p>
        ) : (
          <table className="tabela">
            <thead>
              <tr>
                <th>Conta</th>
                <th>Banco / Tipo</th>
                <th>Agência / Conta</th>
                <th>Saldo Atual</th>
                <th>Projetado ({formatarDataBr(dataCorte)})</th>
              </tr>
            </thead>
            <tbody>
              {resumo.contasBancarias.map((c) => (
                <tr key={c.id}>
                  <td>
                    <strong>{c.nome}</strong>
                    <div className="subtexto-tabela">{c.titular}</div>
                  </td>
                  <td>
                    {c.numBanco} - {c.nomeBanco}
                    <div className="subtexto-tabela">{c.tipoContaDescricao}</div>
                  </td>
                  <td>
                    {c.agencia || c.numConta
                      ? `Ag: ${c.agencia || '-'} | Cc: ${c.numConta || '-'}`
                      : '—'}
                  </td>
                  <td className={c.saldoAtual < 0 ? 'texto-negativo' : 'texto-positivo'}>
                    <strong>{formatarMoeda(c.saldoAtual)}</strong>
                  </td>
                  <td>
                    <strong className={c.saldoProjetado < 0 ? 'texto-negativo' : 'texto-positivo'}>
                      {formatarMoeda(c.saldoProjetado)}
                    </strong>
                    {c.lancamentosPendentesAteCorte > 0 && (
                      <div className="badge-pendente">
                        <Clock size={12} /> {c.lancamentosPendentesAteCorte} pendente(s)
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Bloco 2: Cartões de Crédito (Separados da Liquidez) */}
      <div className="bloco-contas">
        <h3>
          <CreditCard size={19} color="#fba94c" /> Cartões de Crédito (Separados da Liquidez)
        </h3>

        {resumo.cartoesCredito.length === 0 ? (
          <p className="texto-vazio">Nenhum cartão de crédito cadastrado.</p>
        ) : (
          <table className="tabela">
            <thead>
              <tr>
                <th>Cartão</th>
                <th>Instituição</th>
                <th>Titular</th>
                <th>Fatura Atual</th>
                <th>Fatura Projetada ({formatarDataBr(dataCorte)})</th>
              </tr>
            </thead>
            <tbody>
              {resumo.cartoesCredito.map((c) => (
                <tr key={c.id}>
                  <td>
                    <strong>{c.nome}</strong>
                  </td>
                  <td>
                    {c.numBanco} - {c.nomeBanco}
                  </td>
                  <td>{c.titular}</td>
                  <td className="texto-alerta">
                    <strong>{formatarMoeda(c.saldoAtual)}</strong>
                  </td>
                  <td>
                    <strong className="texto-alerta">{formatarMoeda(c.saldoProjetado)}</strong>
                    {c.lancamentosPendentesAteCorte > 0 && (
                      <div className="badge-pendente">
                        <Clock size={12} /> {c.lancamentosPendentesAteCorte} pendente(s)
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}