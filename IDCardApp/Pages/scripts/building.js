define(function () {

    var service = (function (_proxy) {
        var exec = function (cmd, data) {
            if (!_proxy) { throw new Error('非客户端环境'); }

            let p = _proxy.execute(cmd, data && typeof data !== 'string' ? JSON.stringify(data) : data);
            return new Promise((resolve, reject) => {
                p.then(d => {
                    d.data = d && d.jsonData ? JSON.parse(d.jsonData) : null;
                    resolve(d);
                }, e => {
                    Vue.prototype.$message.error('数据请求出错...');
                    console.error(e);
                    reject(e);
                });
            });
        };
        return {
            fetch: function () {
                return exec('repo.building.fetch');
            },
            destory: function (datas) {
                return exec('repo.building.destory', datas);
            },
            create: function (data) {
                return exec('repo.building.create', data);
            },
            update: function (data) {
                return exec('repo.building.update', data);
            }
        };
    })(window['$formProxy']);

    return {
        create: function () {
            var page = new Vue({
                el: '#app',
                data: {
                    sourceData: [
                        { Code: '2016-05-03', Name: '王小虎', Address: '上海市普陀区金沙江路 1518 弄' }
                    ],
                    tableData: [],
                    selections: [],
                    filterVal: ''
                },
                methods: {
                    onModuleClick: function (key) {
                        if (key === 'a') {
                            $formProxy.execute('showDevTools');
                        } else {
                            $formProxy.execute('abcd', ' argument 2')
                                .then(d => console.log(d));
                        }
                    },
                    rowDblclick: function (row, event) {
                        dialog.loadData(row);
                    },
                    handleSelectionChange: function (val) {
                        this.selections = val;
                    },
                    appendRow: function () {
                        dialog.loadData(null);
                    },
                    removeRows: function () {
                        let me = this;
                        let selIds = me.selections.map(t => t.PkId);
                        if (!selIds.length) {
                            return me.$message.warning('请勾选要删除的记录');
                        }
                        me.$confirm('此操作将永久删除所选的 ' + selIds.length + ' 条记录, 是否继续?', '提示', {
                            confirmButtonText: '确定',
                            cancelButtonText: '取消',
                            type: 'warning'
                        }).then(() => service.destory(selIds.join(',')))
                            .then(() => {
                                let s = me.sourceData.filter(t => selIds.indexOf(t.PkId) === -1);
                                me.sourceData = s;
                                me.filter();
                                me.$message.success('删除成功!');
                            });
                    },
                    filter: function () {
                        let data = this.sourceData,
                            val = this.filterVal;
                        if (val) {
                            data = data.filter(t => t.Code.indexOf(val) > -1 || t.Name.indexOf(val) > -1);
                        }
                        this.tableData = data;
                    }
                },
                created: function () {
                    let me = this;
                    me.tableData = me.sourceData;
                    service.fetch().then(d => {
                        if (d.success) {
                            me.sourceData = d.data;
                            me.filter();
                        }
                    });
                }
            });

            var dialog = new Vue({
                el: '#dialog',
                data: {
                    visible: false,
                    bindData: null,
                    formData: {
                        PkId: '',
                        Code: '',
                        Name: '',
                        Address: ''
                    },
                    rules: {
                        Code: [
                            { required: true, message: '请输入代码', trigger: 'blur' }
                        ],
                        Name: [
                            { required: true, message: '请输入名称', trigger: 'blur' }
                        ]
                    }
                },
                methods: {
                    onOk: function () {
                        let me = this;
                        me.$refs['editForm'].validate((valid) => {
                            if (!valid) return false;

                            let data = me.formData;
                            if (me.bindData) {
                                service.update(data).then(
                                    d => {
                                        Object.assign(me.bindData, data);
                                    }
                                );
                            } else {
                                service.create(data).then(
                                    d => {
                                        data.PkId = d.data;
                                        page.sourceData.push(data);
                                        page.filter();
                                    }
                                );
                            }
                            me.visible = false;
                        });
                    },
                    loadData: function (data) {
                        this.bindData = data;
                        Object.assign(this.formData,  data || {
                            PkId: '',
                            Code: '',
                            Name: '',
                            Address: ''
                        });
                        this.visible = true;
                    }
                }
            });
        },
        destory: function () {

        }
    };
});