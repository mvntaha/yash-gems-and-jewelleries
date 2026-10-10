/**
 * Theme: Larkon - Admin & Dashboard Template
 * Author: Techzaa
 * Module/App: Dashboard
 */

class Dashboard {

    constructor() {
        this.$body = document.body;
        this.salesChart = null;
        this.conversionChart = null;
    }

    initCharts() {
        this.initSalesChart();
        this.initConversionChart();
    }

    // 1. Sales / Revenue Chart
    initSalesChart() {
        var performanceChartElement = document.querySelector("#dash-performance-chart");
        if (!performanceChartElement) return;

        var options = {
            series: [], // Will be populated via API
            chart: {
                height: 313,
                type: "line",
                toolbar: {
                    show: false,
                },
            },
            stroke: {
                dashArray: [0, 0],
                width: [0, 2],
                curve: 'smooth'
            },
            fill: {
                opacity: [1, 1],
                type: ['solid', 'gradient'],
                gradient: {
                    type: "vertical",
                    inverseColors: false,
                    opacityFrom: 0.5,
                    opacityTo: 0,
                    stops: [0, 90]
                },
            },
            markers: {
                size: [0, 0],
                strokeWidth: 2,
                hover: {
                    size: 4,
                },
            },
            xaxis: {
                categories: [], // Will be populated
                axisTicks: {
                    show: false,
                },
                axisBorder: {
                    show: false,
                },
            },
            yaxis: [
                {
                    title: {
                        text: 'Orders',
                    },
                    min: 0,
                    axisBorder: {
                        show: false,
                    }
                },
                {
                    opposite: true,
                    title: {
                        text: 'Revenue',
                    },
                    min: 0,
                    axisBorder: {
                        show: false,
                    }
                }
            ],
            grid: {
                show: true,
                strokeDashArray: 3,
                xaxis: {
                    lines: {
                        show: false,
                    },
                },
                yaxis: {
                    lines: {
                        show: true,
                    },
                },
                padding: {
                    top: 0,
                    right: 0,
                    bottom: 0,
                    left: 10,
                },
            },
            legend: {
                show: true,
                horizontalAlign: "center",
                offsetX: 0,
                offsetY: 5,
                markers: {
                    width: 9,
                    height: 9,
                    radius: 6,
                },
                itemMargin: {
                    horizontal: 10,
                    vertical: 0,
                },
            },
            plotOptions: {
                bar: {
                    columnWidth: "30%",
                    barHeight: "70%",
                    borderRadius: 3,
                },
            },
            colors: ["#ff6c2f", "#22c55e"],
            tooltip: {
                shared: true,
                intersect: false,
                y: [{
                    formatter: function (y) {
                        if (typeof y !== "undefined") {
                            return y.toFixed(0);
                        }
                        return y;
                    },
                },
                {
                    formatter: function (y) {
                        if (typeof y !== "undefined") {
                            return "$" + y.toLocaleString('en-IN');
                        }
                        return y;
                    },
                },
                ],
            },
            noData: {
                text: 'Loading...'
            }
        };

        this.salesChart = new ApexCharts(performanceChartElement, options);
        this.salesChart.render();

        // Load initial data
        this.loadSalesData('1Y');

        // Bind filter buttons
        const self = this;
        document.querySelectorAll('.chart-filter').forEach(btn => {
            btn.addEventListener('click', function () {
                // Remove active class from all
                document.querySelectorAll('.chart-filter').forEach(b => b.classList.remove('active'));
                // Add to clicked
                this.classList.add('active');
                // Load data
                self.loadSalesData(this.dataset.period);
            });
        });
    }

    loadSalesData(period) {
        if (!this.salesChart) return;

        fetch(`/Admin/Dashboard/GetSalesChartData?period=${period}`)
            .then(response => response.json())
            .then(data => {
                this.salesChart.updateOptions({
                    xaxis: {
                        categories: data.labels
                    }
                });
                this.salesChart.updateSeries([
                    {
                        name: "Orders",
                        type: "bar",
                        data: data.orders,
                        yAxisIndex: 0
                    },
                    {
                        name: "Revenue",
                        type: "area",
                        data: data.revenue,
                        yAxisIndex: 1
                    }
                ]);
            })
            .catch(err => console.error("Error loading sales chart:", err));
    }

    // 2. Conversion Chart
    initConversionChart() {
        var conversionsElement = document.querySelector("#conversions");
        if (!conversionsElement) return;

        var options = {
            chart: {
                height: 292,
                type: 'radialBar',
            },
            plotOptions: {
                radialBar: {
                    startAngle: -135,
                    endAngle: 135,
                    dataLabels: {
                        name: {
                            fontSize: '14px',
                            color: "undefined",
                            offsetY: 100
                        },
                        value: {
                            offsetY: 55,
                            fontSize: '20px',
                            color: undefined,
                            formatter: function (val) {
                                return val.toFixed(1) + "%";
                            }
                        }
                    },
                    track: {
                        background: "rgba(170,184,197, 0.2)",
                        margin: 0
                    },
                }
            },
            fill: {
                gradient: {
                    enabled: true,
                    shade: 'dark',
                    shadeIntensity: 0.2,
                    inverseColors: false,
                    opacityFrom: 1,
                    opacityTo: 1,
                    stops: [0, 50, 65, 91]
                },
            },
            stroke: {
                dashArray: 4
            },
            colors: ["#ff6c2f", "#22c55e"],
            series: [0], // Initial
            labels: ['Conversion Rate'],
            responsive: [{
                breakpoint: 380,
                options: {
                    chart: {
                        height: 180
                    }
                }
            }],
            grid: {
                padding: {
                    top: 0,
                    right: 0,
                    bottom: 0,
                    left: 0
                }
            },
            noData: {
                text: 'Loading...'
            }
        };

        this.conversionChart = new ApexCharts(conversionsElement, options);
        this.conversionChart.render();

        // Load data
        fetch('/Admin/Dashboard/GetConversionData')
            .then(response => response.json())
            .then(data => {
                this.conversionChart.updateSeries([data.conversionRate]);

                // Update text elements if they exist
                const thisWeekEl = document.getElementById('thisWeekOrders');
                const lastWeekEl = document.getElementById('lastWeekOrders');

                if (thisWeekEl) thisWeekEl.innerText = data.thisWeekOrders;
                if (lastWeekEl) lastWeekEl.innerText = data.lastWeekOrders;
            })
            .catch(err => console.error("Error loading conversion chart:", err));
    }

    init() {
        this.initCharts();
    }
}

class VectorMap {
    initWorldMapMarker() {
        const markerElement = document.querySelector('#world-map-markers');
        if (markerElement) {
            const map = new jsVectorMap({
                map: 'world',
                selector: '#world-map-markers',
                zoomOnScroll: true,
                zoomButtons: false,
                markersSelectable: true,
                markers: [
                    { name: "Canada", coords: [56.1304, -106.3468] },
                    { name: "Brazil", coords: [-14.2350, -51.9253] },
                    { name: "Russia", coords: [61, 105] },
                    { name: "China", coords: [35.8617, 104.1954] },
                    { name: "United States", coords: [37.0902, -95.7129] }
                ],
                markerStyle: {
                    initial: { fill: "#7f56da" },
                    selected: { fill: "#22c55e" }
                },
                labels: {
                    markers: {
                        render: marker => marker.name
                    }
                },
                regionStyle: {
                    initial: {
                        fill: 'rgba(169,183,197, 0.3)',
                        fillOpacity: 1,
                    },
                },
            });
        }
    }

    init() {
        this.initWorldMapMarker();
    }
}

document.addEventListener('DOMContentLoaded', function (e) {
    new Dashboard().init();

    if (document.querySelector('#world-map-markers')) {
        new VectorMap().init();
    }
});