class FareAttribute {
  final String fareId;
  final double? price;
  final String? currencyType;
  final int? paymentMethod;
  final int? transfers;
  final String? agencyId;
  final int? transferDuration;

  FareAttribute({
    required this.fareId,
    this.price,
    this.currencyType,
    this.paymentMethod,
    this.transfers,
    this.agencyId,
    this.transferDuration,
  });

  factory FareAttribute.fromMap(Map<String, dynamic> map) => FareAttribute(
        fareId: map['fare_id'],
        price: (map['price'] as num?)?.toDouble(),
        currencyType: map['currency_type'],
        paymentMethod: map['payment_method'],
        transfers: map['transfers'],
        agencyId: map['agency_id'],
        transferDuration: map['transfer_duration'],
      );

  Map<String, dynamic> toMap() => {
        'fare_id': fareId,
        'price': price,
        'currency_type': currencyType,
        'payment_method': paymentMethod,
        'transfers': transfers,
        'agency_id': agencyId,
        'transfer_duration': transferDuration,
      };
}