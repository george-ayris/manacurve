import d3 from 'd3'

function transformTranslate (x, y) {
  return 'translate(' + x + ',' + y + ')';
}

export default function(chartNode, data, options) {
  var margin = options.margin;
  var chartWidth = options.width - margin.left - margin.right,
    chartHeight = options.barHeight * data.length,
    height = chartHeight + margin.top + margin.bottom;

  var x = d3.scale.linear()
    .domain([0, d3.max(data)])
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

  var bar = innerChart.selectAll('.bar')
    .data(data)

  var barEnter = bar.enter().append('g')
      .attr('class', 'bar')

  if(options.mouseEvents) {
    barEnter
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
  }

  bar
    .attr('transform', function(d, i) { return transformTranslate(0, i * options.barHeight); })
    .classed('clicked', function(d, i) { return i === options.selectedBar; });

  barEnter.append('rect')

  bar.selectAll('rect')
    .data(function(d) { return [d]; })
    .attr('width', x)
    .attr('height', options.barHeight - 1);

  barEnter
    .append('text')
      .attr("dy", ".35em")

  bar.selectAll('text')
    .data(function(d) { return [d]; })
    .attr('x', function(d) { return x(d) + 3; })
    .attr('y', options.barHeight/2)
    .text(function(d) { return d; });

  bar.exit().remove();
};
