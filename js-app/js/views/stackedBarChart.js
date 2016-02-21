import d3 from 'd3'

function transformTranslate (x, y) {
  return 'translate(' + x + ',' + y + ')';
}

export default function(chartNode, data, options) {
  var margin = options.margin;
  var chartWidth = options.width - margin.left - margin.right,
    chartHeight = options.barHeight * data.length,
    height = chartHeight + margin.top + margin.bottom;

  data.forEach(d => {
    var x0 = 0;
    d.bars = d.map((x, i) => { return { x0: x0, x1: x0 += x, cssClass: 'colour' + (i+1) } });
    d.total = x0;
  });

  var x = d3.scale.linear()
    .domain([0, d3.max(data, d => { return d.total; })])
    .range([0, chartWidth]);

  var y = d3.scale.ordinal()
    .domain(data.map(function(_, i) { return options.indexToLabel(i); }))
    .rangeBands([0, chartHeight], .1);

  var yAxis = d3.svg.axis()
    .scale(y)
    .outerTickSize(0)
    .orient('left');

  var singleData = [1];
  var chart = d3.select(chartNode)

  chart
    .attr('width', options.width)
    .attr('height', height)

  var innerChart = chart
    .selectAll('g')
    .data(singleData)

  innerChart.enter()
    .append('g')

  innerChart.attr('transform', transformTranslate(margin.left, margin.top));

  var yAxisContainer = innerChart.selectAll('.y.axis')
    .data(singleData)

  yAxisContainer.enter().append('g')
        .attr('class', 'y axis')
        .attr('transform', transformTranslate(-10, 0))

  yAxisContainer.call(yAxis);

  var row = innerChart.selectAll('.row')
      .data(data);

  row.enter().append('g')
    .attr('class', 'row');

  row
    .attr('transform', function(d, i) { return transformTranslate(0, i * options.barHeight); })
    .classed('clicked', function(d, i) { return i === options.selectedBar; })
    .on('mouseover', function() {
      d3.select(this)
        .classed('mouseover', true);
    })
    .on('mouseout', function() {
      d3.select(this)
        .classed('mouseover', false);
    })
    .on('mousedown', function(d, i) {
      options.barClicked(i);
    });

  row.exit().remove();

  var text = row.selectAll('text')
      .data(d => { return [d]; });

  text.enter().append('text')
    .attr("dy", ".35em");

  text
    .attr('x', d => { console.log(d); return x(d.total) + 3; })
    .attr('y', options.barHeight/2)
    .text(d => { return d; });

  text.exit().remove();

  var rect = row.selectAll('rect')
      .data(d => { return d.bars; });

  rect.enter().append('rect');

  rect
    .attr('width', d => { return x(d.x1) - x(d.x0); })
    .attr('x', d => { return x(d.x0); })
    .attr('height', options.barHeight - 1) // TODO: y.rangeBand()?
    .attr('class', d => { return d.cssClass; });

  rect.exit().remove();
};
